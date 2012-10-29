SQL Extractor
=============

CLI Tool to extract View/StoredProc/...-Definitions from an SQL-Server to track them in a versioning system

## Why

I needed an easy tool to track the development of a MS-SQL database. Primary to track changes to views and stored procedures. 
This is the result of this need - just a quick hack. But maybe it might useful for others too.

Maybe I'll extend this tool for further usecases - or you can also fork and extend it as you wish.

With SQL_Extractor I have now a quick method to dump all/some definitions to separate files and track them combined with the application source-code with my code versioning system (git in my case)

## Command Line
(all options in one line)

    SQL_Extractor 
	   connection=<string>          -- use connection string to specify database
       server=<hostname/servername> -- needed, if no connection string is given
       db=<databasename>            -- needed, if no connection string is given
       user=<user>                  -- use system user if omitted
       pwd=<password>               -- needed, if user is set
       dest=<folder>                -- output folder, if omitted, current folder is used
       filter=<name LIKE clause>    -- only export objects which name matches this filter
       type=<type LIKE clause>      -- only export object with this types
       where=<additional WHERE clause>

	   header=<TRUE/false>          -- add own header to view
	   header_date=<true/FALSE>     -- include date of export in header, att. when using diff. tools
	   versions=<n>                 -- keep n versions of old files (numbered, 1...n, n=oldest); 0 (default) means overwrite

The *filter* and *type* arguments can be specified more than once - they are OR'ed together.

The *type* filter is matched against the sys.object.type column - described here: [MSDN](http://msdn.microsoft.com/en-us/library/ms190324.aspx)

The *where* argument can be used to fine-tune the filter. Following SQL-Command is used to get the definitions:
 
    SELECT OBJECT_NAME(sql_modules.object_id) as name, sql_modules.*, objects.type, objects.type_desc 
    FROM sys.sql_modules WITH (NOLOCK) 
    LEFT JOIN sys.objects WITH (NOLOCK) ON objects.object_id=sql_modules.object_id 

So you have following columns available:

	name
	object_id	
	definition	
	uses_ansi_nulls	
	uses_quoted_identifier	
	is_schema_bound	
	uses_database_collation	
	is_recompiled	
	null_on_null_input	
	execute_as_principal_id	
	type	
	type_desc


## Configuration Files
Instead of setting all options in the commandline, you can also specify a file which contains all settings:
    
    SQL_Extractor <filename.sql_extract>

(Extension must be **.sql_extract**)

The file can contain the same arguments as the commandline, in the same format as above

    <parameter>=<setting>
    eg.: db=MyDatabase

Further, you can define more than one set of extract-options, separated by following entry: 

    [Second Extractor]
    
see examples below.

## Examples

**By commandline:**

    SQL_Extractor.exe server=localhost db=myData dest=out


Will dump all definitions of all Views/StoredProcs/Triggers/... of the Database *myData* into the subfolder *out* in the current directory.

**Via config File:**

    SQL_Extractor.exe cfg.sql_extract


Content of *cfg.sql_extract*

    server=NOTEBOOK\SQLEXPRESS
    db=AdventureWorks2012
    pwd=MySecret
    user=Foo
    dest=c:\tmp\sql\test1

    type=VIEW
    filter=my%
    filter=%other%

Will extract all VIEWs which name starts with "**my**" or contains "**other**" into the folder *c:\tmp\sql\test1*

Content of *cfg.sql_extract*, joining more definitions

    server=NOTEBOOK\SQLEXPRESS
    db=AdventureWorks2012
    pwd=MySecret
    user=Foo
    dest=c:\tmp\sql\test1

    type=VIEW
    filter=my%
    filter=%other%

    [Server2]
    server=Other\SQLEXPRESS
    db=Database2
    pwd=MyOtherSecret
    user=Bar
    dest=c:\tmp\sql\test2

    type=%
    filter=%


License
=======

[GPLv3](http://www.gnu.org/licenses/gpl-3.0.en.html)