# IIS Log Importer
This is a console app that reads IIS Log files and imports them to a SQL Table. If the table does not exist it creates a new once calld Logs

## Configuration

The `app.config` file has settings for:

- `ApplicationName`: This is the name of application the logs belong to and is recorded in the database
- `LogDirectory`: This is the location of the IIS Logs
- Connectionstring `Logs`: this is the database that the logs will be imported to

## Output and logging
If there is a problem parsing a log file (e.g. it contains a field that is not configured) a record will recorded in the database with the CanParse flag set to false.

If there are any errors during processing they are recorded (along with the date and time of the log) in log.txt created in the root directory.

The Console window logs as it begins to parse each file, however, the application will stop logging for a long time, during this phase it is catching up and recording the logs in the database. Once complete a message will ask you to "press any key to close".

## Maintenance
The most likely changes to this are for adding additional fields. To do this you need to make the following changes:

1. Add the new field(s) as properties to `LogFileLine.cs`
2. Near the top of `LogFileParser.cs` there is a dictionary called `FieldMapping`. This contains an action for each field that populates the correct property in `LogFileLine`. You must add a dictionary item with corresponding Action to for each field.
3. Add the new field(s) to the `TABLE_CREATE_SQL` and `INSERT_LOG_SQL` in `Repository.cs`

## Parallelism 
To improve performance the reading of the log files and storing of the data is multithreaded. This has been acheived using the [Microsoft TPL Dataflow Library](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library).

There are 3 blocks:
1. Read the file paths
2. Parse the files into `LogFileLine`objects
3. Save the `LogFileLine` objecs to database

As step 3 is the slowest step it has been given the highest degree of Parallelism