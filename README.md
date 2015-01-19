About
=====

Project site: https://github.com/timabell/ef-document-generator

Reads a db schema and then adds/updates the documentation in the matching `.emdx` file.

This is a fork of http://eftsqldocgenerator.codeplex.com/ fixed-up for EF5

Usage
=====

	EFTSQLDocumentation.Generator.exe 	\
		-c "server=.;database=yourdatabase;Integrated Security=SSPI"  \
		-i path\to\your\Model.edmx

Arguments
---------

* -c, --connectionString... ConnectionString of the documented database
* -i, --input... original edmx file
* -o, --output [optional] ... output edmx file - Default : original edmx file

What now?
---------

You may want to get the comments to be added to your generated model classes.
You can do that by modifying your model.tt file as follows: https://gist.github.com/timabell/74bb6c6bbb7a4de843dc

With that in place you will have consistent model documentation across your
schema, edmx and c# model, with the tooling to keep them in sync.

Licence
=======

Apache 2.0 as per upstream: http://eftsqldocgenerator.codeplex.com/license


Download
========

Get the binary from https://github.com/timabell/ef-document-generator/releases/latest

Links
=====

* Why? Because: http://stackoverflow.com/questions/2747788/how-can-i-make-the-entity-data-model-designer-use-my-database-column-descriptions
