using System.Runtime.CompilerServices;
using NLog;

// Constents
const bool IS_UNIX = true;
const bool REMOVE_DUPLICATES = true;

const string DELIMETER_1 = ",";
const string DELIMETER_2 = "|";
const string START_END_TITLE_WITH_DELIMETER1_INDICATOR = "\"";

const int PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT = 1_000; //Tested, >~ 1,000 line before removal, use int.MaxValue for infinity, int's length is max for used lists
const ushort DEVICE_MAX_TITLE_LENGTH = 105; //Hardcoded value dependent on terminal, sets upper limit for title length
const ushort DEVICE_MAX_GENRE_SECTION_LENGTH = 55; //Hardcoded value dependent on terminal, sets upper limit for title length

string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Movies_No_Filter),
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Movies_Filter),
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Movies),
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Exit)};

string[] FILTER_OPTIONS_IN_ORDER = { enumToStringFilterMenuWorkArround(FILTER_MENU_OPTIONS.Year),
                                     enumToStringFilterMenuWorkArround(FILTER_MENU_OPTIONS.Title),
                                     enumToStringFilterMenuWorkArround(FILTER_MENU_OPTIONS.Genre),
                                     enumToStringFilterMenuWorkArround(FILTER_MENU_OPTIONS.Exit)};

Movie.GENRES[] ALL_MOVIE_GENRES = (Movie.GENRES[])Enum.GetValues(typeof(Movie.GENRES));

// Info
int lastId = 0;//Should never be negative, but not uint for allowing -1 for error checking

string loggerPath = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "nlog.config";
string moviesRecordsPath = Directory.GetCurrentDirectory() + (IS_UNIX ? "/" : "\\") + "movies.csv";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(loggerPath).GetCurrentClassLogger();

logger.Info("Main program is running and log mager is started, program is running on a " + (IS_UNIX ? "" : "non-") + "unix-based device.");

List<int> moviesTitleYearHash = new List<int>();//Store data hashes for speed, stored centrally. TODO: Move out if needed


string optionsSelector(string[] options)
{
    string userInput;
    int selectedNumber;
    bool userInputWasImproper = true;
    List<int> cleanedListIndexs = new List<int> {};
    string optionsTextAsStr = ""; //So only created once. Requires change if adjustable width is added

    for (int i = 0; i < options.Length; i++)
    {
        // options[i] = options[i].Trim();//Don't trim so when used, spaces can be used to do spaceing
        if (options[i] != null && options[i].Replace(" ", "").Length > 0)
        {//Ensure that not empty or null
            cleanedListIndexs.Add(i);//Add index to list
            optionsTextAsStr = $"{optionsTextAsStr}\n{string.Format($" {{0,{options.Length.ToString().Length}}}) {{1}}", cleanedListIndexs.Count(), options[i])}";//Have to use this as it prevents the constents requirment.
        }
    }
    optionsTextAsStr = optionsTextAsStr.Substring(1); //Remove first \n

    // Seprate from rest by adding a blank line
    Console.WriteLine();
    do
    {
        Console.WriteLine("Please select an option from the following...");
        Console.WriteLine(optionsTextAsStr);
        Console.Write("Please enter an option from the list: ");
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
    // Seprate from rest by adding a blank line
    Console.WriteLine();
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
    else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Movies_No_Filter))
    {
        presentListRange(movies);
    }
    else if (menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Movies_Filter))
    {
        bool presentRange = true;
        List<Movie> filteredMovies = movies.GetRange(0, movies.Count);

        string option = optionsSelector(FILTER_OPTIONS_IN_ORDER);

        if (option == enumToStringFilterMenuWorkArround(FILTER_MENU_OPTIONS.Exit)){
            presentRange = false;
        }else if (option == enumToStringFilterMenuWorkArround(FILTER_MENU_OPTIONS.Year)){
            string userInputRaw;
            int userChoosenInteger;

            int startingFilterYear, endingFilterYear;

            bool yearIsInvalid;
            do
            {
                Console.Write("Please enter the starting year to include in results: ");
                userInputRaw = Console.ReadLine().Trim();
                if (int.TryParse(userInputRaw, out userChoosenInteger) || userInputRaw.Length == 0) //Duplicate .Length == 0 checking to have code in the same location
                {
                    if (userInputRaw.Length == 0)
                    {
                        logger.Error("Your year choice cannot be be empty. Please enter a positve integer.");
                        yearIsInvalid = true;
                    }
                    else if (userChoosenInteger < 0)
                    {
                        logger.Error("Your choosen year choice was not a positive integer, please try again.");
                        yearIsInvalid = true;
                    }
                    else
                    {
                        yearIsInvalid = false;
                    }
                }
                else
                {
                    //User response was not a integer
                    logger.Error("Your choosen year choice was not a integer, please try again.");
                    yearIsInvalid = true; //Was not an integer
                }
            } while (yearIsInvalid);
            startingFilterYear = userChoosenInteger;

            //Now ask for ending date
            do
            {
                Console.Write($"Please enter the ending year to include in results ({startingFilterYear}-____), or leave blank to confine results to only the starting year: ");
                userInputRaw = Console.ReadLine().Trim();
                if (int.TryParse(userInputRaw, out userChoosenInteger) || userInputRaw.Length == 0) //Duplicate .Length == 0 checking to have code in the same location
                {
                    if (userInputRaw.Length == 0)
                    { //This time blank means just use starting year
                        userChoosenInteger = startingFilterYear;
                        yearIsInvalid = false;
                    }
                    else if (userChoosenInteger < 0)
                    {
                        logger.Error("Your choosen year choice was not a positive integer, please try again.");
                        yearIsInvalid = true;
                    }
                    else if (userChoosenInteger < startingFilterYear)
                    {
                        logger.Warn("Your choosen year choice was less than your starting year, swapping start and stop years.");
                        int tempStore = startingFilterYear;
                        startingFilterYear = userChoosenInteger;
                        userChoosenInteger = tempStore; //Will set to endingFilterYear after do-while
                        yearIsInvalid = false;
                    }
                    else
                    {
                        yearIsInvalid = false;
                    }
                }
                else
                {
                    //User response was not a integer
                    logger.Error("Your choosen year choice was not a integer, please try again.");
                    yearIsInvalid = true; //Was not an integer
                }
            } while (yearIsInvalid);
            endingFilterYear = userChoosenInteger;

            // TODO: Start more thread and parallise searching?
            // foreach(Movie movie in filteredMovies)
            for (int i = 0; i < filteredMovies.Count;)
            {
                Movie movie = filteredMovies[i];
                if (movie.Year >= startingFilterYear && movie.Year <= endingFilterYear){
                    i++;
                }else{
                    filteredMovies.Remove(movie);
                }
            }
            filteredMovies.Sort(); //Sort by year and then by title as filtered by title
        }else if (option == enumToStringFilterMenuWorkArround(FILTER_MENU_OPTIONS.Title)){
            string userInputRaw;
            string userSearchStr = null;
            do
            {
                Console.Write("Please enter the text you would like to search for in the title: ");
                userInputRaw = Console.ReadLine().Trim();
                if(userInputRaw.Length == 0){
                    logger.Error("Your search text choice cannot be be empty. Please enter more text.");
                }else{
                    userSearchStr = userInputRaw;
                }
            }while(userSearchStr == null);
            
            // TODO: Start more thread and parallise searching?
            for (int i = 0; i < filteredMovies.Count;)
            {
                Movie movie = filteredMovies[i];
                if(movie.Title.Contains(userSearchStr, StringComparison.InvariantCultureIgnoreCase)){ //Use InvariantCultureIgnoreCase to include things like é's and ö's
                    i++;
                }else{
                    filteredMovies.Remove(movie);
                }
            }
            //Sort by title and then by year as filtered by title
            filteredMovies.Sort((movie1, movie2) => movie1.CompareToTitle(movie2, true));
        }else if(option == enumToStringFilterMenuWorkArround(FILTER_MENU_OPTIONS.Genre)){
            Movie.GENRES[] filterByGenres = repeatingGenreOptionsSelector(false, true).ToArray();
            // TODO: Start more thread and parallise searching?
            for (int i = 0; i < filteredMovies.Count; ){
                Movie movie = filteredMovies[i];
                bool containsAllSelectedGenres = true;
                foreach(Movie.GENRES genre in filterByGenres){
                    if(movie.Genres.Contains(genre)){
                    }else{
                        containsAllSelectedGenres = false;
                        break;
                    }
                }
                if(containsAllSelectedGenres){ //Use InvariantCultureIgnoreCase to include things like é's and ö's
                    i++;
                }else{
                    filteredMovies.Remove(movie);
                }
            }
        }

        // string[] remainingGenresAsStrings = new string[allMovieGenres.Length]; // -1 to remove error enum but then +1 for the exit option
        // for(int i = 0; i < remainingGenresAsStrings.Length; i++){
        //     remainingGenresAsStrings[i] = Movie.GenresEnumToString(allMovieGenres[i]);
        // }
        // remainingGenresAsStrings[remainingGenresAsStrings.Length-1] = "Done entering genres";
        // optionsSelector()

        if(presentRange){
            presentListRange(filteredMovies);
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
        do
        {
            Console.Write("Please enter the name of the new movie: ");
            userInputRaw = Console.ReadLine().Trim();
            movieTitle = userInputRaw.Trim();
            if (movieTitle.Length == 0)
            {
                logger.Error("Movie name cannot be left blank, please try again.");
            }
            else
            {
                movieTitleFailed = false;
            }

            if (userInputRaw.Contains(","))
            {
                movieTitle = $"\"{userInputRaw}\"";
            }
        } while (movieTitleFailed);

        // Movie year
        int currentYear = int.Parse($"{DateTime.Now:yyyy}");

        bool yearIsInvalid;
        do
        {
            Console.Write($"To use movie year \"{currentYear}\", leave blank, else enter integer now: ");
            userInputRaw = Console.ReadLine().Trim();
            if (int.TryParse(userInputRaw, out userChoosenInteger) || userInputRaw.Length == 0) //Duplicate .Length == 0 checking to have code in the same location
            {
                if (userInputRaw.Length == 0 || userChoosenInteger == currentYear)
                { //Skip check if using auto year, manually typed or by entering blank
                    userChoosenInteger = currentYear;
                    yearIsInvalid = false;
                }
                else if (userChoosenInteger < 0)
                {
                    logger.Error("Your choosen year choice was not a positive integer, please try again.");
                    yearIsInvalid = true;
                }
                else
                {
                    yearIsInvalid = false;
                }
            }
            else
            {
                //User response was not a integer
                logger.Error("Your choosen year choice was not a integer, please try again.");
                yearIsInvalid = true; //Was not an integer
            }
        } while (yearIsInvalid);
        int movieYear = userChoosenInteger;

        int newId = lastId + 1; //Assume last record id is not out of order, avoid using auto id for placing with repeat id's that may have existed before but then were removed. Option avaiable if manually entering id.
        do
        {
            Console.Write($"To use movie id \"{newId}\", leave blank, else enter integer now: ");
            userInputRaw = Console.ReadLine().Trim();
            if (int.TryParse(userInputRaw, out userChoosenInteger) || userInputRaw.Length == 0) //Duplicate .Length == 0 checking to have code in the same location
            {
                if (userInputRaw.Length == 0 || userChoosenInteger == newId)
                { //Skip check if using auto id, manually typed or by entering blank
                    userChoosenInteger = newId;
                    lastId++;//Increment last id
                }
                else if (userChoosenInteger < 0)
                {
                    logger.Error("Your choosen id choice was not a positive integer, please try again.");
                    userChoosenInteger = -1;
                }
                else
                {
                    // TODO: Make more efficent
                    foreach (Movie movie in movies) // Check if id is already used
                    {
                        if (movie.Id == userChoosenInteger)
                        {
                            logger.Error("Your choosen id is already in use, please try again.");
                            userChoosenInteger = -1;
                        }
                    }
                }
            }
            else
            {
                //User response was not a integer
                logger.Error("Your choosen id choice was not a integer, please try again.");
                userChoosenInteger = -1; //Was not an integer
            }
        } while (userChoosenInteger == -1);

        // Genres selection
        // List<Movie.GENRES> newMovieGenres = new List<Movie.GENRES>{};
        string newMovieGenresStr = "";


        Movie.GENRES[] selectedGenres = Movie.SortGenres(repeatingGenreOptionsSelector(true, false));
        
        foreach(Movie.GENRES genre in selectedGenres){
            newMovieGenresStr = $"{newMovieGenresStr}{DELIMETER_2}{Movie.GenresEnumToString(genre)}";
        }


    if (newMovieGenresStr.Length > 2)
    { //Means that at least one item was choosen
        newMovieGenresStr = newMovieGenresStr.Substring(1); //Remove first DELEMITER_2, should always trigger
    }else{
        //Add in empty identifer
        newMovieGenresStr = $"{Movie.GenresEnumToString(Movie.GENRES.NO_GENRES_LISTED)}";
    }


        int movieId = userChoosenInteger;

        //Write the record
        // TODO, ensue no errors with SW!
        try
        {
            StreamWriter sw = new StreamWriter(moviesRecordsPath, true);

            if (movieTitle.EndsWith("\""))
            {//Merge year with title (some exisiting records do not have a year, but going forward, all should so it's included here)
                movieTitle = $"{movieTitle.Substring(0, movieTitle.Length - 2)} ({movieYear})\"";
            }
            else
            {
                movieTitle = $"{movieTitle} ({movieYear})";
            }

            // Inform user that movie was created and added    
            Movie newMovie = new Movie(movieId, movieTitle, newMovieGenresStr, DELIMETER_2);
            if (REMOVE_DUPLICATES)
            {
                //Check hashtable for existing combination and add
                int movieTitleYearHash = newMovie.GetHashCode();
                if (moviesTitleYearHash.Contains(movieTitleYearHash))
                {
                    logger.Warn($"Dupliate movie record on movie \"{newMovie.Title}\" with id \"{newMovie.Id}\". Not adding to records.");
                }
                else
                {
                    movies.Add(newMovie);
                    moviesTitleYearHash.Add(movieTitleYearHash);
                    sw.WriteLine($"{movieId}{DELIMETER_1}{movieTitle}{DELIMETER_1}{newMovieGenresStr}");
                    sw.Close();
                    Console.WriteLine($"Added movie \"{newMovie.Title.Replace("\"","")}\" under id \"{newMovie.Id}\" with {newMovie.Genres.Length} genre identifier{(newMovie.Genres.Length>1? "s": "")}{(newMovie.Genres[0]==Movie.GENRES.NO_GENRES_LISTED? " (having none is included as a an identifier of being empty)" : "")}.");
                }
            }
            else
            {
                movies.Add(newMovie);
                sw.WriteLine($"{movieId}{DELIMETER_1}{movieTitle}{DELIMETER_1}{newMovieGenresStr}");
                sw.Close();
                Console.WriteLine($"Added movie \"{newMovie.Title}\" under id \"{newMovie.Id}\" with {newMovie.Genres.Length} genre identifier{(newMovie.Genres.Length>1? "s": "")}{(newMovie.Genres[0]==Movie.GENRES.NO_GENRES_LISTED? " (having none is included as a an identifier of being empty)" : "")}.");
            }
        }
        catch (FileNotFoundException ex)
        {
            logger.Fatal($"The file, '{moviesRecordsPath}' was not found.");
        }
        catch (Exception ex)
        {
            logger.Fatal(ex.Message);
        }

    }
    else
    {
        logger.Fatal("Somehow, menuCheckCommand was slected that did not fall under the the existing commands, this should never have been triggered. Improper menuCheckCommand is getting through");
    }

}


void presentListRange(List<Movie> moviesList)
{
    string[] options = new string[moviesList.Count / PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT + (moviesList.Count % PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT - 1 > 0 ? 1 : 0) + 1];// +1 is for exit
    int[,] optionsRanges = new int[options.Length - 1, 2]; //-2 for no range at options[0], 2 is for range start and range end.  //TODO: Combine arrays so that they aren't needed to be in sync? It's verry temporary and there would be more processing to create and then need to pull out and cast or create new class, ect.
    // TODO: AUTO FOR LESS THAN PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT
    options[0] = "Exit without printing report.";
    int recordsRangeStart;
    int recordsRangeEnd;
    for (int i = 0; i < options.Length - 2; i++) //-2 to exclude last range
    {
        recordsRangeStart = i * PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT;
        recordsRangeEnd = (i + 1) * PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT - 1;
        optionsRanges[i, 0] = recordsRangeStart;
        optionsRanges[i, 1] = recordsRangeEnd;
        options[i + 1] = $"List movie results range {recordsRangeStart}-{recordsRangeEnd}";
    }
    recordsRangeStart = (options.Length - 2) * PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT;
    options[options.Length - 1] = $"List results range {recordsRangeStart}-{moviesList.Count}";
    if (options.Length >= 2)
    {
        optionsRanges[options.Length - 2, 0] = (options.Length - 2) * PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT;
        optionsRanges[options.Length - 2, 1] = moviesList.Count;
    }
    else
    {
        options[options.Length - 1] = "Exit, no records were found with those criteia.";
    }

    string optionStringSelected = optionsSelector(options);
    if (optionStringSelected == options[0])
    {//Always quit option

    }
    else
    {
        for (int i = 0; i < options.Length - 1; i++)
        {//Start at 0 as quit option already taken care of but optionsRanges is one fewer
            if (optionStringSelected == options[i + 1])
            {
                int[] rangeStats = getStats(moviesList, optionsRanges[i, 0], optionsRanges[i, 1] - 1); //Remove one from end as records start on 1, not 0
                displayMoviesFromList(moviesList, optionsRanges[i, 0], optionsRanges[i, 1], (ushort)rangeStats[0], (ushort)rangeStats[2], (ushort)rangeStats[3]);
            }
        }
    }
}


List<Movie.GENRES> repeatingGenreOptionsSelector(bool exclusivity, bool includeErrorEnum){
    string[] remainingGenresAsStrings = new string[ALL_MOVIE_GENRES.Length + (includeErrorEnum? 1 : 0)]; // -1 to remove error enum but then +1 for the exit option

    List<Movie.GENRES> selectedGenres = new List<Movie.GENRES>(){};
    
    string genreSelectedStr = "";

    // Build remainingGenresAsStrings
    for (int i = 0; i < ALL_MOVIE_GENRES.Length; i++)
    {
        remainingGenresAsStrings[i] = Movie.GenresEnumToString(ALL_MOVIE_GENRES[i]);
    }
    remainingGenresAsStrings[remainingGenresAsStrings.Length - 1] = "Done entering genres";

    do{
        genreSelectedStr = optionsSelector(remainingGenresAsStrings);
        for (int i = 0; i < remainingGenresAsStrings.Length; i++)
        {
            if (genreSelectedStr == remainingGenresAsStrings[i])
            {
                if (genreSelectedStr == remainingGenresAsStrings[remainingGenresAsStrings.Length - 1])
                { //Last item was added just above as not an enum, but to exit
                    genreSelectedStr = null; //Inform that do-while is over
                }
                else if ( exclusivity && genreSelectedStr == Movie.GenresEnumToString(Movie.GENRES.NO_GENRES_LISTED))
                { //Exit early that none are listed
                    selectedGenres.Add(Movie.GENRES.NO_GENRES_LISTED);//Should be the only element
                    genreSelectedStr = null; //Inform that do-while is over
                }
                else
                {
                    selectedGenres.Add(Movie.GetEnumFromString(genreSelectedStr));
                }
                remainingGenresAsStrings[i] = null; //Blank options are removed from options selector
                i = remainingGenresAsStrings.Length;
            }
        }
        remainingGenresAsStrings[remainingGenresAsStrings.Length - 2] = null; //Remove no genres listed on the first round
    } while (genreSelectedStr != null); //Last index is done option

    return selectedGenres;
}

List<Movie> buildMoviesListFromFile(string dataPath)
{
    List<Movie> moviesInFile = new List<Movie>();

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
        if (!recordIsBroken)
        {
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
            if (REMOVE_DUPLICATES)
            {
                //Check hashtable for existing combination and add
                int movieTitleYearHash = movie.GetHashCode();
                if (moviesTitleYearHash.Contains(movieTitleYearHash))
                {
                    logger.Warn($"Dupliate movie record on movie \"{movie.Title.Replace("\"", "")}\" with id \"{movie.Id}\", year \"{movie.Year}\". Not including in results.");
                }
                else
                {
                    moviesInFile.Add(movie);
                    moviesTitleYearHash.Add(movieTitleYearHash);
                }
            }
            else
            {
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

    // Adjust to ensure fit within screen
    longestTitle = Math.Min(longestTitle, DEVICE_MAX_TITLE_LENGTH);

    // After list is created, display results to user.
    char headerDividerNode = '+';
    char headerDividerLinkVert = '|';
    char headerDividerLinkHorz = '-';

    string headerDividerLine = $"{headerDividerNode}";
    string headerTitlesLine = "Movie Title";

    string headerGenreSegment = "Genres";

    //Have to use string.Format() as it prevents the constents requirment.
    headerTitlesLine = string.Format($"{headerDividerLinkVert}{{0,{(longestTitle + headerTitlesLine.Length) / 2}}}", headerTitlesLine);
    headerTitlesLine = string.Format($"{{0,-{longestTitle + 1}}}{headerDividerLinkVert}{{1,-{Movie.YEAR_SPACE_FOR_DIGIT_PLACES}}}{headerDividerLinkVert}", headerTitlesLine, "Year"); //+1 is so that the first link spacer is taken into account

    headerGenreSegment = string.Format($"{{0,-{(longestGenresRawLength + (longestGenresLengthCount - 1) * 2 + headerGenreSegment.Length) / 2}}}", headerGenreSegment);
    headerGenreSegment = string.Format($"{{0,{Math.Min(longestGenresRawLength + (longestGenresLengthCount - 1) * 2 + 1, DEVICE_MAX_GENRE_SECTION_LENGTH)}}}", headerGenreSegment);
    headerTitlesLine = $"{headerTitlesLine}{headerGenreSegment}{headerDividerLinkVert}";

    for (int i = 0; i < Math.Min(longestTitle, DEVICE_MAX_TITLE_LENGTH); i++) { headerDividerLine += headerDividerLinkHorz; }// = is so that the first link spacer is taken into account
    headerDividerLine = $"{headerDividerLine}{headerDividerNode}";
    for (int i = 0; i < Movie.YEAR_SPACE_FOR_DIGIT_PLACES; i++) { headerDividerLine += headerDividerLinkHorz; }

    headerDividerLine = $"{headerDividerLine}{headerDividerNode}";

    for (int i = 0; i < Math.Min(longestGenresRawLength + (longestGenresLengthCount - 1) * 2 + 1, DEVICE_MAX_GENRE_SECTION_LENGTH); i++) { headerDividerLine += headerDividerLinkHorz; } //Take into account, space for ", ", +1 is because of the added " " beforehand

    headerDividerLine = $"{headerDividerLine}{headerDividerNode}";

    Console.WriteLine(); //Give space before report
    // Display header
    Console.WriteLine(headerDividerLine);
    Console.WriteLine(headerTitlesLine);
    Console.WriteLine(headerDividerLine);

    for (int i = recordStartNum; i < recordEndNum; i++)
    {
        Movie movie = movieList[i];//Does not like uint, TODO: Make list take larger list or tranfer to diffrent data structure.
        Console.Write(string.Format($"{headerDividerLinkVert}{{0,-{longestTitle}}}|{{1,{Movie.YEAR_SPACE_FOR_DIGIT_PLACES}}}|", movie.Title, (movie.Year == -1 ? ""/*"NTAV"*/ : movie.Year)));//Have to use this as it prevents the constents requirment.
        string genreDisplay = " ";
        if (movie.Genres.Length == 1 && movie.Genres[0] == Movie.GENRES.NO_GENRES_LISTED)
        {
            // If no no genres are listed
        }
        else
        {
            foreach (Movie.GENRES genre in movie.Genres)
            {
                genreDisplay = $"{genreDisplay}{Movie.GenresEnumToString(genre)}, ";
            }
            if (genreDisplay.Length > 1)
            {
                genreDisplay = genreDisplay.Substring(0, genreDisplay.Length - 2);
            }
        }
        Console.WriteLine(string.Format($"{{0,-{Math.Min(longestGenresRawLength + (longestGenresLengthCount - 1) * 2 + 1, DEVICE_MAX_GENRE_SECTION_LENGTH)}}}|", genreDisplay));
    }
    Console.WriteLine(headerDividerLine);
    Console.WriteLine(); //Give space after report
}

int[] getStats(List<Movie> movieList, int listStartIndex = 0, int listEndIndex = -1)
{
    int[] longestStats = new int[4];
    longestStats[0] = 0; //Longest Title
    longestStats[1] = 0; //Largest # of genres
    longestStats[2] = 0; //Longest total genre length raw (no connectors)
    longestStats[3] = 0; //Genres # of longest total genre length raw 


    listStartIndex = Math.Clamp(listStartIndex, 0, movieList.Count - 1);
    if (listEndIndex == -1 || listEndIndex >= movieList.Count) { listEndIndex = movieList.Count - 1; }
    for (int i = listStartIndex; i <= listEndIndex; i++)
    {
        longestStats[0] = Math.Max(longestStats[0], movieList[i].Title.Length);
        longestStats[1] = Math.Max(longestStats[1], movieList[i].Genres.Length);
        int totalMovieGenreLength = 0;
        foreach (Movie.GENRES genre in movieList[i].Genres) { totalMovieGenreLength += Movie.GenresEnumToString(genre).Length; }
        if (totalMovieGenreLength > longestStats[2])
        {
            longestStats[2] = totalMovieGenreLength;
            longestStats[3] = movieList[i].Genres.Length;
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
        MAIN_MENU_OPTIONS.View_Movies_No_Filter => $"View movies on file in order (display max ammount is {PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT:N0})",
        MAIN_MENU_OPTIONS.View_Movies_Filter => $"Filter movies on file",
        MAIN_MENU_OPTIONS.Add_Movies => "Add movies to file",
        _ => "ERROR"
    };
}
string enumToStringFilterMenuWorkArround(FILTER_MENU_OPTIONS filterMenuEnum)
{
    return filterMenuEnum switch
    {
        FILTER_MENU_OPTIONS.Exit => "Quit Filtering",
        FILTER_MENU_OPTIONS.Year => "By year",
        FILTER_MENU_OPTIONS.Title => "By title",
        FILTER_MENU_OPTIONS.Genre => "By genre",
        _ => "ERROR"
    };
}

public enum MAIN_MENU_OPTIONS
{
    Exit,
    View_Movies_No_Filter,
    View_Movies_Filter,
    Add_Movies
}
public enum FILTER_MENU_OPTIONS
{
    Exit,
    Year,
    Title,
    Genre
    // Id
}