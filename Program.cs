﻿using NLog;

// Constents
const bool IS_UNIX = true;
const bool REMOVE_DUPLICATES = true;

const string DELIMETER_1 = ",";
const string DELIMETER_2 = "|";
const string START_END_TITLE_WITH_DELIMETER1_INDICATOR = "\"";

const int PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT = 1_000; //Tested, >~ 1,000 line before removal, use int.MaxValue for infinity, int's length is max for used lists


string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Movies),
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Movies),
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Exit)};

// Info
int lastId = 0;//Should never be negative, but not uint for allowing -1 for error checking

string loggerPath        = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "nlog.config";
string moviesRecordsPath = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "movies.csv";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(loggerPath).GetCurrentClassLogger();

logger.Info("Main program is running and log mager is started, program is running on a " + (IS_UNIX ? "" : "non-") + "unix-based device.");


string optionsSelector(string[] options)
{
    string userInput;
    int selectedNumber;
    bool userInputWasImproper = true;
    List<int> cleanedListIndexs = new List<int>{};
    string optionsTextAsStr = ""; //So only created once. Requires change if adjustable width is added

    for (int i = 0; i < options.Length; i++)
    {
        // options[i] = options[i].Trim();//Don't trim so when used, spaces can be used to do spaceing
        if(options[i] != null && options[i].Replace(" ","").Length > 0){//Ensure that not empty or null
            cleanedListIndexs.Add(i);//Add index to list
            optionsTextAsStr = $"{optionsTextAsStr}\n{string.Format($" {{0,{options.Length.ToString().Length}}}) {{1}}", cleanedListIndexs.Count(), options[i])}";//Have to use this as it prevents the constents requirment.
        }
    }
    optionsTextAsStr = optionsTextAsStr.Substring(1); //Remove first \n
    
    do
    {
        Console.WriteLine("Please select an option from the following...");
        Console.WriteLine(optionsTextAsStr);
        Console.Write("Please enter a option from the list: ");
        userInput = Console.ReadLine().Trim();

        //TODO: Move to switch without breaks instead of ifs or if-elses?
        if (!int.TryParse(userInput, out selectedNumber))
        {// User response was not a integer
            logger.Error("Your selector choice was not a integer, please try again.");
        }
        else if (selectedNumber < 1 || selectedNumber > cleanedListIndexs.Count()) //Is count because text input index starts at 1
        {// User response was out of bounds
            logger.Error($"Your selector choice was not within bounds, please try again. (Range is 1-{cleanedListIndexs.Count()})");
        }
        else
        {
            userInputWasImproper = false;
        }
    } while (userInputWasImproper);
    return options[cleanedListIndexs[selectedNumber - 1]];
}


List<Movie> movies = buildMoviesListFromFile(moviesRecordsPath);
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
        int[,] optionsRanges = new int[options.Length-1,2]; //-2 for no range at options[0], 2 is for range start and range end.  //TODO: Combine arrays so that they aren't needed to be in sync? It's verry temporary and there would be more processing to create and then need to pull out and cast or create new class, ect.
        // TODO: AUTO FOR LESS THAN PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT
        options[0] = "Exit without printing report.";
        int recordsRangeStart;
        int recordsRangeEnd;
        for (int i = 0; i < options.Length - 2; i++) //-2 to exclude last range
        {
            recordsRangeStart = i * PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT;
            recordsRangeEnd   = (i + 1) * PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT - 1;
            optionsRanges[i,0] = recordsRangeStart;
            optionsRanges[i,1] = recordsRangeEnd;
            options[i+1] = $"List movies range {recordsRangeStart}-{recordsRangeEnd}";
        }
        recordsRangeStart = (options.Length - 2) * PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT;
        options[options.Length - 1] = $"List movies range {recordsRangeStart}-{movies.Count}";
        optionsRanges[options.Length-2,0] = (options.Length - 2) * PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT;
        optionsRanges[options.Length-2,1] = movies.Count;

        string optionStringSelected = optionsSelector(options);
        if(optionStringSelected == options[0]){//Always quit option
            
        }else{
            for(int i=0; i<options.Length-1; i++){//Start at 0 as quit option already taken care of but optionsRanges is one fewer
                if(optionStringSelected == options[i+1]){
                    int[] rangeStats = getStats(movies, optionsRanges[i,0], optionsRanges[i,1]-1); //Remove one from end as records start on 1, not 0
                    displayMoviesFromList(movies, optionsRanges[i,0], optionsRanges[i,1], (ushort)rangeStats[0], (ushort)rangeStats[2], (ushort)rangeStats[3]);
                }
            }
        }
    }
    else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Movies))
    {

        Console.WriteLine("\n  -~:<{[( Add to records )]}>:~-\n");
        string userInputRaw;
        int userChoosenInteger;

        // Movie title
        string movieTitle;
        bool movieTitleFailed = true;
        do{
            Console.Write("Please enter the name of the new movie: ");
            userInputRaw = Console.ReadLine().Trim();
            movieTitle = userInputRaw.Trim();
            if(movieTitle.Length == 0){
                logger.Error("Movie name cannot be left blank, please try again.");
            }else{
                movieTitleFailed = false;
            }

            if(userInputRaw.Contains(",")){
                movieTitle = $"\"{userInputRaw}\"";
            }
        }while(movieTitleFailed);

        // Movie year
        int currentYear = int.Parse($"{DateTime.Now:yyyy}");

        bool yearIsInvalid;
        do{
            Console.Write($"To use movie year \"{currentYear}\", leave blank, else enter integer now: ");
            userInputRaw = Console.ReadLine().Trim();
            if (int.TryParse(userInputRaw, out userChoosenInteger) || userInputRaw.Length == 0) //Duplicate .Length == 0 checking to have code in the same location
            {
                if(userInputRaw.Length == 0 || userChoosenInteger == currentYear){ //Skip check if using auto year, manually typed or by entering blank
                    userChoosenInteger = currentYear;
                    yearIsInvalid = false;
                }else if(userChoosenInteger < 0){
                    logger.Error("Your choosen year choice was not a positive integer, please try again.");
                    yearIsInvalid = true;
                }else{
                    yearIsInvalid = false;
                }
            }else{
                //User response was not a integer
                logger.Error("Your choosen id choice was not a integer, please try again.");
                yearIsInvalid = true; //Was not an integer
            }
        }while(yearIsInvalid);
        int movieYear = userChoosenInteger;

        int newId = lastId + 1; //Assume last record id is not out of order, avoid using auto id for placing with repeat id's that may have existed before but then were removed. Option avaiable if manually entering id.
        do{
            Console.Write($"To use movie id \"{newId}\", leave blank, else enter integer now: ");
            userInputRaw = Console.ReadLine().Trim();
            if (int.TryParse(userInputRaw, out userChoosenInteger) || userInputRaw.Length == 0) //Duplicate .Length == 0 checking to have code in the same location
            {
                if(userInputRaw.Length == 0 || userChoosenInteger == newId){ //Skip check if using auto id, manually typed or by entering blank
                    userChoosenInteger = newId;
                    lastId++;//Increment last id
                }else if(userChoosenInteger < 0){
                    logger.Error("Your choosen id choice was not a positive integer, please try again.");
                    userChoosenInteger = -1;
                }else{
                    // TODO: Make more efficent
                    foreach (Movie movie in movies) // Check if id is already used
                    {
                        if(movie.id == userChoosenInteger){
                            logger.Error("Your choosen id is already in use, please try again.");
                            userChoosenInteger = -1;
                        }
                    }
                }
            }else{
                //User response was not a integer
                logger.Error("Your choosen id choice was not a integer, please try again.");
                userChoosenInteger = -1; //Was not an integer
            }
        }while(userChoosenInteger == -1);

        // Genres selection
        // List<Movie.GENRES> newMovieGenres = new List<Movie.GENRES>{};
        string newMovieGenresStr = "";

        Movie.GENRES[] allMovieGenres = (Movie.GENRES[])Enum.GetValues(typeof(Movie.GENRES));
        string[] remainingGenresAsStrings = new string[allMovieGenres.Length]; // -1 to remove error enum but then +1 for the exit option
        for(int i = 0; i < remainingGenresAsStrings.Length; i++){
            remainingGenresAsStrings[i] = Movie.GenresEnumToString(allMovieGenres[i]);
        }
        remainingGenresAsStrings[remainingGenresAsStrings.Length-1] = "Done entering genres";

        string genreSelectedStr;
        do{
            genreSelectedStr = optionsSelector(remainingGenresAsStrings);
            newMovieGenresStr = $"{newMovieGenresStr}{DELIMETER_2}{genreSelectedStr}";
            for(int i = 0; i < remainingGenresAsStrings.Length; i++){
                if(genreSelectedStr == remainingGenresAsStrings[i]){
                    remainingGenresAsStrings[i] = null; //Blank options are removed from options selector
                    i = remainingGenresAsStrings.Length;
                }
            }
            Console.WriteLine("genreSelectedStr = "+genreSelectedStr);
        }while(genreSelectedStr != remainingGenresAsStrings[remainingGenresAsStrings.Length-1]); //Last index is done option
        if(newMovieGenresStr.Length > 2){
            newMovieGenresStr = newMovieGenresStr.Substring(0, newMovieGenresStr.Length-2); //Remove last DELEMITER_2
        }
        

        // optionsSelector();
        int movieId = userChoosenInteger;

        //Write the record
// TODO, ensue no errors with SW!
        try{
            StreamWriter sw = new StreamWriter(moviesRecordsPath, true);

            if(movieTitle.EndsWith("\"")){//Merge year with title (some exisiting records do not have a year, but going forward, all should so it's included here)
                movieTitle = $"{movieTitle.Substring(0,movieTitle.Length-2)} ({movieYear})\"";
            }else{
                movieTitle = $"{movieTitle} ({movieYear})";
            }
            sw.WriteLine($"{movieId}{DELIMETER_1}{movieTitle}{DELIMETER_1}{newMovieGenresStr}");
            movies.Add(new Movie(movieId, movieTitle, newMovieGenresStr, DELIMETER_2));
            sw.Close();
        }catch(FileNotFoundException ex){
            logger.Fatal($"The file, '{moviesRecordsPath}' was not found.");
        }catch(Exception ex){
            logger.Fatal(ex.Message);
        }

    }else{
        logger.Fatal("Somehow, menuCheckCommand was slected that did not fall under the the existing commands, this should never have been triggered. Improper menuCheckCommand is getting through");
    }

}

List<Movie> buildMoviesListFromFile(string dataPath)
{
    List<Movie> moviesInFile = new List<Movie>();
    List<int> moviesTitleYearHash = new List<int>();//Store data hashes for speed

    // Info for tracking
    uint lineNumber = 1;//Should never be negative, so uint

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
        string movieTitle = "";
        if (!recordIsBroken){
            movieTitle = movieParts[1];
            if (movieTitle.Length == 0 || movieTitle == " ")
            {
                logger.Error($"Broken movie record on line #{lineNumber} (\"{line}\"). Movie title is empty. Movie title cannot be blank or empty. !!!!!" + movieTitle + "!!!!!");
                recordIsBroken = true;
            }
        }

        if (!recordIsBroken)
        {
            string genres = movieParts[2];
            Movie movie = new Movie(movieId, movieTitle, genres, DELIMETER_2);
            if(REMOVE_DUPLICATES){
                //Check local hashtable for existing combination and add
                int movieTitleYearHash = movie.GetHashCode();
                if(moviesTitleYearHash.Contains(movieTitleYearHash)){
                    logger.Warn($"Dupliate movie record on movie \"{movie.title}\" with id \"{movie.id}\". Not including in results.");
                }else{
                    moviesInFile.Add(movie);
                    moviesTitleYearHash.Add(movieTitleYearHash);
                }
            }else{
                moviesInFile.Add(movie);
            }
            
            // Console.WriteLine(movie);
        }

        // Update helpers
        lineNumber++;
        lastId = Math.Max(lastId, movieId);
    }
    sr.Close();
    return moviesInFile;
}

void displayMoviesFromList(List<Movie> movieList, int recordStartNum, int recordEndNum, ushort longestTitle, ushort longestGenresRawLength, ushort longestGenresLengthCount)
{
    // After list is created, display results to user.
    char headerDividerNode = '+';
    char headerDividerLinkVert = '|';
    char headerDividerLinkHorz = '-';
                                            
    string headerDividerLine = $"{headerDividerNode}";
    string headerTitlesLine = "Movie Title";

    string headerGenreSegment = "Genres";

    //Have to use string.Format() as it prevents the constents requirment.
    headerTitlesLine = string.Format($"{headerDividerLinkVert}{{0,{(longestTitle + headerTitlesLine.Length) / 2}}}", headerTitlesLine);
    headerTitlesLine = string.Format($"{{0,-{longestTitle+1}}}{headerDividerLinkVert}{{1,-{Movie.YEAR_SPACE_FOR_DIGIT_PLACES}}}{headerDividerLinkVert}", headerTitlesLine, "Year"); //+1 is so that the first link spacer is taken into account
    
    headerGenreSegment = string.Format($"{{0,-{(longestGenresRawLength+(longestGenresLengthCount-1)*2+headerGenreSegment.Length)/2}}}",headerGenreSegment);
    headerGenreSegment = string.Format($"{{0,{longestGenresRawLength+(longestGenresLengthCount-1)*2+1}}}",headerGenreSegment);
    headerTitlesLine = $"{headerTitlesLine}{headerGenreSegment}{headerDividerLinkVert}";

    for (int i = 0; i < longestTitle; i++) { headerDividerLine += headerDividerLinkHorz; }// = is so that the first link spacer is taken into account
    headerDividerLine = $"{headerDividerLine}{headerDividerNode}";
    for (int i = 0; i < Movie.YEAR_SPACE_FOR_DIGIT_PLACES; i++) { headerDividerLine += headerDividerLinkHorz; }

    headerDividerLine = $"{headerDividerLine}{headerDividerNode}";

    Console.WriteLine("longestGenresRawLength = "+longestGenresRawLength);
    for (int i = 0; i < longestGenresRawLength+(longestGenresLengthCount-1)*2+1; i++) { headerDividerLine += headerDividerLinkHorz; } //Take into account, space for ", ", +1 is because of the added " " beforehand

    headerDividerLine = $"{headerDividerLine}{headerDividerNode}";

    Console.WriteLine(); //Give space before report
    // Display header
    Console.WriteLine(headerDividerLine);
    Console.WriteLine(headerTitlesLine);
    Console.WriteLine(headerDividerLine);

    // movieList.Sort();
    for(int i = recordStartNum; i < recordEndNum; i++ )
    {
        Movie movie = movieList[i];//Does not like uint, TODO: Make list take larger list or tranfer to diffrent data structure.
        Console.Write(string.Format($"{headerDividerLinkVert}{{0,-{longestTitle}}}|{{1,{Movie.YEAR_SPACE_FOR_DIGIT_PLACES}}}|", movie.title, (movie.year == -1? ""/*"NTAV"*/ : movie.year)));//Have to use this as it prevents the constents requirment.
        string genreDisplay = " ";
        if(movie.genres.Length == 1 && movie.genres[0] == Movie.GENRES.NO_GENRES_LISTED){
            // If no no genres are listed
        }else{
            foreach(Movie.GENRES genre in movie.genres){
                genreDisplay = $"{genreDisplay}{Movie.GenresEnumToString(genre)}, ";
            }
            if(genreDisplay.Length > 1){
                genreDisplay = genreDisplay.Substring(0,genreDisplay.Length-2);
            }
        }
        Console.WriteLine(string.Format($"{{0,-{longestGenresRawLength+(longestGenresLengthCount-1)*2+1}}}|", genreDisplay));
    }
    Console.WriteLine(headerDividerLine);
    Console.WriteLine(); //Give space after report
}

int[] getStats(List<Movie> movieList, int listStartIndex=0, int listEndIndex=-1){
    int[] longestStats = new int[4];
    longestStats[0] = 0; //Longest Title
    longestStats[1] = 0; //Largest # of genres
    longestStats[2] = 0; //Longest total genre length raw (no connectors)
    longestStats[3] = 0; //Genres # of longest total genre length raw 


    listStartIndex = Math.Clamp(listStartIndex, 0, movieList.Count-1);
    if(listEndIndex == -1 || listEndIndex >= movieList.Count){ listEndIndex = movieList.Count-1; }
    for(int i = listStartIndex; i <= listEndIndex; i++)
    {
        longestStats[0] = Math.Max(longestStats[0], movieList[i].title.Length);
        longestStats[1] = Math.Max(longestStats[1], movieList[i].genres.Length);
        int totalMovieGenreLength = 0;
        foreach(Movie.GENRES genre in movieList[i].genres){ totalMovieGenreLength += Movie.GenresEnumToString(genre).Length; }
        if(totalMovieGenreLength > longestStats[2]){
            longestStats[2] = totalMovieGenreLength;
            longestStats[3] = movieList[i].genres.Length;
        }
    }
    return longestStats;
}


// vvv UNUM STUFF vvv

string enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS mainMenuEnum)
{

    return mainMenuEnum switch
    {
        MAIN_MENU_OPTIONS.Exit => "Quit program",
        MAIN_MENU_OPTIONS.View_Movies => $"View movies on file (display max ammount is {PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT})",
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