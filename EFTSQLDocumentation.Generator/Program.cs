using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommandLineParser.Arguments;
using CommandLineParser.Exceptions;

namespace EFTSQLDocumentation.Generator
{

    class Program : IDisposable
    {
        static void Main(string[] args)
        {
            CommandLineParser.CommandLineParser parser = CreateParser();

            try
            {
                parser.ParseCommandLine(args);
            }
            catch (CommandLineException e)
            {
                Console.WriteLine(e.Message);
                parser.ShowUsage();
                return;
            }


            String connectionString = ((ValueArgument<SqlConnectionStringBuilder>)parser.LookupArgument("connectionString")).Value.ConnectionString;
            String inputFileName = ((FileArgument)parser.LookupArgument("input")).Value.FullName;
            String outputFileName = ((FileArgument)parser.LookupArgument("output")).Value != null ?
                                        ((FileArgument)parser.LookupArgument("output")).Value.FullName :
                                        inputFileName;

            Program p = new Program(connectionString, inputFileName, outputFileName);

            p.CreateDocumentation();
            p.Dispose();
        }
        private static CommandLineParser.CommandLineParser CreateParser()
        {
            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
            ValueArgument<SqlConnectionStringBuilder> connectionStringArgument = new ValueArgument<SqlConnectionStringBuilder>('c', "connectionString", "ConnectionString of the documented database")
            {
                Optional = false,
                ConvertValueHandler = (stringValue) =>
                {
                    SqlConnectionStringBuilder connectionStringBuilder;
                    try
                    {
                        connectionStringBuilder = new SqlConnectionStringBuilder(stringValue);
                    }
                    catch
                    {
                        throw new InvalidConversionException("invalid connection string", "connectionString");
                    }
                    if (String.IsNullOrEmpty(connectionStringBuilder.InitialCatalog))
                    {
                        throw new InvalidConversionException("no InitialCatalog was specified", "connectionString");
                    }

                    return connectionStringBuilder;
                }
            };
            FileArgument inputFileArgument = new FileArgument('i', "input", "original edmx file") { FileMustExist = true, Optional = false };
            FileArgument outputFileArgument = new FileArgument('o', "output", "output edmx file - Default : original edmx file") { FileMustExist = false, Optional = true };

            parser.Arguments.Add(connectionStringArgument);
            parser.Arguments.Add(inputFileArgument);
            parser.Arguments.Add(outputFileArgument);

            parser.IgnoreCase = true;
            return parser;
        }


        public String ConnectionString { get; set; }
        public String InputFileName { get; set; }
        public String OutputFileName { get; set; }

        private SqlConnection _connection;

        public Program(String connectionString, String inputFileName, String outputFileName)
        {
            this.ConnectionString = connectionString;
            this.InputFileName = inputFileName;
            this.OutputFileName = outputFileName;

            this._connection = new SqlConnection(connectionString);
            this._connection.Open();
        }
        public void Dispose()
        {
            this._connection.Dispose();
        }

        private void CreateDocumentation()
        {
            XDocument doc = XDocument.Load(this.InputFileName);

            if (doc.Root == null)
            {
                throw new Exception(string.Format("Loaded XDocument Root is null. File: {0}", InputFileName));
            }
            var entityTypeElements = doc.FindByLocalName("EntityType");

            int i = 0;
            foreach (XElement entityTypeElement in entityTypeElements)
            {
                String tableName = entityTypeElement.Attribute("Name").Value;
                var propertyElements = entityTypeElement.FindByLocalName("Property");

                Console.Clear();
                Console.WriteLine("Analyzing table {0} of {1}", i++, entityTypeElements.Count());
                Console.WriteLine(" => TableName : {0}" +
                                  "\n => property count : {1}", tableName, propertyElements.Count());

                this.AddNodeDocumentation(entityTypeElement, GetTableDocumentation(tableName));

                foreach (XElement propertyElement in propertyElements)
                {
                    String columnName = propertyElement.Attribute("Name").Value;
                    this.AddNodeDocumentation(propertyElement, GetColumnDocumentation(tableName, columnName));
                }
            }

            Console.WriteLine("Writing result to {0}", this.OutputFileName);
            if (File.Exists(this.OutputFileName))
                File.Delete(this.OutputFileName);
            doc.Save(this.OutputFileName);
        }
        private void AddNodeDocumentation(XElement element, String documentation)
        {
            // remove stale documentation
            element.FindByLocalName("Documentation").Remove();

            if (String.IsNullOrEmpty(documentation))
                return;
            var xmlns = element.GetDefaultNamespace();

            element.AddFirst(new XElement(xmlns + "Documentation", new XElement(xmlns + "Summary", documentation)));
        }
        private String GetTableDocumentation(String tableName)
        {
            using (SqlCommand command = new SqlCommand(@" SELECT [value] 
                                                          FROM fn_listextendedproperty (
                                                                'MS_Description', 
                                                                'schema', 'dbo', 
                                                                'table',  @TableName, 
                                                                null, null)", this._connection))
            {

                command.Parameters.AddWithValue("TableName", tableName);

                return command.ExecuteScalar() as String;
            }
        }
        private String GetColumnDocumentation(String tableName, String columnName)
        {
            using (SqlCommand command = new SqlCommand(@"SELECT [value] 
                                                         FROM fn_listextendedproperty (
                                                                'MS_Description', 
                                                                'schema', 'dbo', 
                                                                'table', @TableName, 
                                                                'column', @columnName)", this._connection))
            {

                command.Parameters.AddWithValue("TableName", tableName);
                command.Parameters.AddWithValue("ColumnName", columnName);

                return command.ExecuteScalar() as String;
            }
        }
    }
}
