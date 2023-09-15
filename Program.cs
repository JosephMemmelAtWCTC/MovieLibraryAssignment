﻿using NLog;

// Constents
bool IS_UNIX = true;
const string DELIMETER_1 = ",";
const string DELIMETER_2 = "|";
const string START_END_TITLE_WITH_DELIMETER1_INDICATOR = "\"";

const uint PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT = 1_000; //Tested, >~ 1,000 line before removal


string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Movies),
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Movies),
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Exit)};


string loggerPath = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "nlog.config";
string moviesPath = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "movies.csv";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(loggerPath).GetCurrentClassLogger();

logger.Info("Main program is running and log mager is started, program is running on a " + (IS_UNIX ? "" : "non-") + "unix-based device.");

string optionsSelector(string[] options)
{
    string userInput;
    int selectedNumber = -1;
    bool userInputWasImproper = true;
    do
    {
        Console.WriteLine("Please select an option from the following...");
        for (int i = 1; i <= options.Length; i++)
        {
            // Console.WriteLine($"  {i,options.Length.ToString().Length}) {options[i - 1]}");
            Console.WriteLine(string.Format($" {{0,{options.Length.ToString().Length}}}) {{1}}", i, options[i - 1]));//Have to use this as it prevents the constents requirment.

        }
        Console.Write("Please enter a option from the list: ");
        userInput = Console.ReadLine();

        //TODO: Move to switch without breaks instead of ifs or if-elses?
        if (!int.TryParse(userInput, out selectedNumber))
        {// User response was not a integer
            logger.Error("Your selector choice was not a integer, please try again.");
        }
        else if (selectedNumber < 1 || selectedNumber > options.Length)
        {// User response was out of bounds
            logger.Error($"Your selector choice was not within bounds, please try again. (Range is 1-{options.Length})");
        }
        else
        {
            userInputWasImproper = false;
        }
    } while (userInputWasImproper);
    return options[selectedNumber - 1];
}


List<Movie> movies = buildMoviesListFromFile(moviesPath);
if (movies == null)
{
    logger.Fatal("There was a problem accessing the provided file. Closing program..."); //Does not give path again.
    return;
}

while (true)
{

    // TODO: Move to switch with integer of place value and also make not relient on index by switching to enum for efficiency
    string menuCheckCommand = optionsSelector(MAIN_MENU_OPTIONS_IN_ORDER);

    if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Exit))
    {//If user intends to exit the program
        logger.Info("Program quiting...");
        return;
    }
    else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Movies))
    {
        string[] options = new string[movies.Count / PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT + (movies.Count % PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT-1 > 0 ? 1 : 0) + 1];// +1 is for exit
        int[,] optionsRanges = new int[options.Length-1,2];//-2 for no range at options[0], 2 is for range start and range end.  //TODO: Combine arrays so that they aren't needed to be in sync? It's verry temporary and there would be more processing to create and then need to pull out and cast or create new class, ect.
        // TODO: AUTO FOR LESS THAN PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT
        options[0] = "Exit without printing report.";
        int recordsRangeStart;
        int recordsRangeEnd;
        for (int i = 0; i < options.Length - 2; i++)//-2 to exclude last range
        {
            recordsRangeStart = (int) (i * PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT);
            recordsRangeEnd   = (int) ((i + 1) * PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT - 1);
            optionsRanges[i,0] = recordsRangeStart;
            optionsRanges[i,1] = recordsRangeEnd;
            options[i+1] = $"List movies range {recordsRangeStart}-{recordsRangeEnd}";
        }
        recordsRangeStart = (int) ((options.Length - 2) * PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT);
        options[options.Length - 1] = $"List movies range {recordsRangeStart}-{movies.Count}";
        optionsRanges[options.Length-2,0] = (int) ((options.Length - 2) * PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT);
        optionsRanges[options.Length-2,1] = movies.Count;

        string optionStringSelected = optionsSelector(options);
        if(optionStringSelected == options[0]){//Always quit option
            
        }else{
            for(int i=0; i<options.Length-1; i++){//Start at 0 as quit option already taken care of but optionsRanges is one fewer
                if(optionStringSelected == options[i+1]){
                    displayMoviesFromList(movies, optionsRanges[i,0], optionsRanges[i,1], getLongestTitle(movies, optionsRanges[i,0], optionsRanges[i,1]-1)); //Remove one from end as records start on 1, not 0
                }
            }
        }
    }
    else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Movies))
    {

    }
    else
    {
        logger.Fatal("Somehow, menuCheckCommand was slected that did not fall under the the existing commands, this should never have been triggered. Improper menuCheckCommand is getting through");
    }

}

List<Movie> buildMoviesListFromFile(string dataPath)
{
    List<Movie> moviesInFile = new List<Movie>();
    // Info
    uint lineNumber = 1;//Should never be negative, so uint
    // Metrics
    ushort longestTitle = 0;

    // ALL TERMINATORS
    if (!System.IO.File.Exists(dataPath))
    {
        logger.Fatal($"The file, '{dataPath}' was not found.");
        // throw new FileNotFoundException();
        return null;
    }
    // Take care of the rest of at this point all unknown filesystem errors (not accessable, ect.)
    StreamReader sr;
    try
    {
        sr = new StreamReader(dataPath);
    }
    catch (Exception ex)
    {
        logger.Fatal(ex.Message);
        // throw new Exception($"Problem using file at \"{dataPath}\"");
        return null;
    }

    while (!sr.EndOfStream)
    {
        bool recordIsBroken = true;
        string line = sr.ReadLine();
        // string[] movieParts = line.Substring(0, line.IndexOf(DELIMETER_1));
        string[] movieParts = line.Split(DELIMETER_1);
        if (movieParts.Length > 3 && (line.Substring(line.IndexOf(DELIMETER_1)).Split(START_END_TITLE_WITH_DELIMETER1_INDICATOR).Length - 1 >= 2))
        {//Assume first that quotation marks are used to lower
            ushort indexOfFirstDelimeter1 = (ushort)(line.IndexOf(DELIMETER_1) + 1);//Can be ushort as line above makes sure cannot be -1
            ushort indexOfLastDelimeter1 = (ushort)line.Substring(indexOfFirstDelimeter1).LastIndexOf(DELIMETER_1);//Can be ushort as line above makes sure cannot be -1
            movieParts[1] = line.Substring(indexOfFirstDelimeter1, indexOfLastDelimeter1).Replace(START_END_TITLE_WITH_DELIMETER1_INDICATOR, "");
            movieParts[2] = movieParts[movieParts.Length - 1];//Get last element that was split using delimeter #1
            movieParts = new string[] { movieParts[0], movieParts[1], movieParts[2] };
        }

        if (movieParts.Length <= 2)
        {
            logger.Error($"Broken movie record on line #{lineNumber} (\"{line}\"). Not enough arguments provided on line. Must have a id, a title, and optionally genres.");
        }
        else if (movieParts.Length > 3)
        {
            logger.Error("movieParts=" + movieParts.Length + $"Broken movie record on line #{lineNumber} (\"{line}\"). Too many arguments provided on line. Must have a id, a title, and optionally genres.");
        }
        else
        {
            recordIsBroken = false;
        }
        if (!int.TryParse(movieParts[0], out int movieId))
        {
            logger.Error($"Broken movie record on line #{lineNumber} (\"{line}\"). Movie id is not a integer. Movie id must be a integer.");
            recordIsBroken = true;
        }
        string movieTitle = movieParts[1];
        if (movieTitle.Length == 0 || movieTitle == " ")
        {
            logger.Error($"Broken movie record on line #{lineNumber} (\"{line}\"). Movie title is empty. Movie title cannot be blank or empty. !!!!!" + movieTitle + "!!!!!");
            recordIsBroken = true;
        }

        string genres = movieParts[2];
        if (!recordIsBroken)
        {
            Movie movie = new Movie(movieId, movieTitle, genres, DELIMETER_2);
            moviesInFile.Add(movie);
            // Console.WriteLine(movie);
            // Update metrics
            longestTitle = Math.Max(longestTitle, (ushort)movie.title.Length);
        }

        // Update helpers
        lineNumber++;
        // if(lineNumber > 99){
        //     break;
        // }
    }
    sr.Close();
    return moviesInFile;
}

void displayMoviesFromList(List<Movie> movieList, int recordStartNum, int recordEndNum, int longestTitle)
{
    // After list is created, display results to user.
    char headerDividerNode = '+';
    char headerDividerLink = '|';
                                            
    string headerDividerLine = $"{headerDividerNode}";
    string headerTitlesLine = "Movie Title";

    headerTitlesLine = string.Format($"{headerDividerLink}{{0,{(longestTitle + headerTitlesLine.Length) / 2}}}", headerTitlesLine);//Have to use this as it prevents the constents requirment.
    headerTitlesLine = string.Format($"{{0,-{longestTitle+1}}}{headerDividerLink}{{1,-{Movie.YEAR_SPACE_FOR_DIGIT_PLACES}}}{headerDividerLink}", headerTitlesLine, "Year"); //+1 is so that the first link spacer is taken into account //Have to use this as it prevents the constents requirment.

    for (int i = 0; i < longestTitle; i++) { headerDividerLine += "-"; }// = is so that the first link spacer is taken into account
    headerDividerLine = $"{headerDividerLine}{headerDividerNode}";
    for (int i = 0; i < Movie.YEAR_SPACE_FOR_DIGIT_PLACES; i++) { headerDividerLine += "-"; }

    headerDividerLine = $"{headerDividerLine}{headerDividerNode}";

    Console.WriteLine(); //Give space before report
    // Display header
    Console.WriteLine(headerDividerLine);
    Console.WriteLine(headerTitlesLine);
    Console.WriteLine(headerDividerLine);
    for(int i = recordStartNum; i < recordEndNum; i++ )
    {
        Movie movie = movieList[i];//Does not like uint, TODO: Make list take larger list or tranfer to diffrent data structure.
        Console.WriteLine(string.Format($"{headerDividerLink}{{0,-{longestTitle}}}|{{1,{Movie.YEAR_SPACE_FOR_DIGIT_PLACES}}}|", movie.title, (movie.year == -1? ""/*"NTAV"*/ : movie.year)));//Have to use this as it prevents the constents requirment.
    }
    Console.WriteLine(headerDividerLine);
    Console.WriteLine(); //Give space after report

}

ushort getLongestTitle(List<Movie> movieList, int listStartIndex=0, int listEndIndex=-1){
    int currentLongestTitle = 0;
    listStartIndex = Math.Clamp(listStartIndex, 0, movieList.Count-1);
    if(listEndIndex == -1 || listEndIndex >= movieList.Count){ listEndIndex = movieList.Count-1; }
    for(int i = listStartIndex; i <= listEndIndex; i++)
    {
        currentLongestTitle = Math.Max(currentLongestTitle, movieList[i].title.Length);
    }
    return (ushort) currentLongestTitle;// Most effient to cast here instead of inside caculations
}


// vvv UNUM STUFF vvv
string enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS mainMenuEnum)
{

    return mainMenuEnum switch
    {
        MAIN_MENU_OPTIONS.Exit => "Quit program",
        MAIN_MENU_OPTIONS.View_Movies => "View all movies on file",
        MAIN_MENU_OPTIONS.Add_Movies => "Add movies to file",
        _ => "ERROR"
    };

}

public enum MAIN_MENU_OPTIONS
{
    Exit,
    View_Movies,
    Add_Movies
}