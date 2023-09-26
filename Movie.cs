using Microsoft.AspNetCore.Diagnostics;

public class Movie : IEquatable<Movie>, IComparable<Movie>
{
    public enum GENRES
    {
        ACTION,
        ADVENTURE,
        ANIMATION,
        CHILDRENS,
        COMEDY,
        CRIME,
        DOCUMENTARY,
        DRAMA,
        FANTASY,
        FILM_NOIR,
        HORROR,
        MUSICAL,
        MYSTERY,
        ROMANCE,
        SCI_FI,
        THRILLER,
        WAR,
        WESTERN,
        NO_GENRES_LISTED,
        ERROR_NOT_A_VALID_GENRE
    }

    //TODO: Make programatic in future versions
    public const bool CONVERT_GREGORIAN_TO_HUMAN_ERA = false;
    public const byte YEAR_SPACE_FOR_DIGIT_PLACES = CONVERT_GREGORIAN_TO_HUMAN_ERA ? 5 : 4;
    public const bool ALL_INPUT_YEARS_ARE_HUMAN_ERA = false;

    public int Id { get; }// Can add set later if editing answer is implemented, appears can have duplicate movies with diffrent id's, do not use for filtering

    public string Title { get; }

    public int Year { get; }//Allowed to be negative for indicating not set, in case of using other year system other then gregorian this implementation requires it still to be positive

    public GENRES[] Genres { get; }


    public Movie(int id, string title, /*params*/ string[] genres)
    {//Cannot have "params" with overloaded method also uses strings there
        this.Id = id;
        title = title.Trim();
        //If title follows specifc format "______(####)", seperate out year
        short lastOpeningParenthesieTitleIndex = (short)title.LastIndexOf("(");
        short lastClosingParenthesieTitleIndex = (short)title.LastIndexOf(")");
        // TODO: Decide to throw error if date is not included, it is not a field and decided as optional, not throwing/logging error for now as movies.csv includes quite a lot that do not include the year
        if (lastClosingParenthesieTitleIndex == title.Length - 1 //Closing parenthesies must be at the end of string, also checks for Last is ")...(" instead of "(...)", determine not including a title
           && lastClosingParenthesieTitleIndex - lastOpeningParenthesieTitleIndex - 2 > 1 && lastClosingParenthesieTitleIndex - lastOpeningParenthesieTitleIndex - 2 <= YEAR_SPACE_FOR_DIGIT_PLACES //Number of charaters taking up must be at least 1, max YEAR_SPACE_FOR_DIGIT_PLACES, else determine does not include a title
           && short.TryParse(title.Substring(lastOpeningParenthesieTitleIndex + 1, lastClosingParenthesieTitleIndex - lastOpeningParenthesieTitleIndex - 1), out short year)
           )
        {
            this.Title = title.Substring(0, title.Length - (title.Length - lastOpeningParenthesieTitleIndex)).TrimEnd();//Beginning has already been trimmed, only need to check the end
            if (CONVERT_GREGORIAN_TO_HUMAN_ERA && !ALL_INPUT_YEARS_ARE_HUMAN_ERA && year < 10_000)
            {//Assumes that input is gregorian, unless year is at least 10,000
                this.Year = 10_000 + year;
            }
            else
            {
                this.Year = year;
            }
        }
        else
        {//Particular title does contain year, skip removal from title
            this.Title = title;
            this.Year = -1;
        }
        this.Genres = new GENRES[genres.Length];
        for (int i = 0; i < genres.Length; i++)
        {
            this.Genres[i] = GetEnumFromString(genres[i]);
        }
        // Sort
        this.Genres = SortGenres(this.Genres.ToList());

    }
    public Movie(int id, string title, string genreStringList, string genreStringDelimiter)
        : this(id, title, genreStringList.Split(genreStringDelimiter))
    { }

    public override string ToString(){
        return "Id: " + Id + " Title: " + Title + " Year: " + Year;
    }
    public override int GetHashCode(){
        // Hash code does not include genres or id, used for sorting out duplicates
        return $"{this.Title}_{this.Year}".GetHashCode();
    }

    public override bool Equals(object obj){
        if (obj == null) return false;
        Movie objAsMovie = obj as Movie;
        if (objAsMovie == null) return false;
        else return Equals(objAsMovie);
    }
    public bool Equals(Movie other){
        if (other == null) return false;
        return (this.GetHashCode().Equals(other.GetHashCode()));
    }

    // Default comparer for Movie (year decending then alphabetically decending)
    public int CompareTo(Movie compareMovie){
        return CompareToYear(compareMovie, true);
    }


    // Secondary comparer for Movie (year decending)
    public int CompareToYear(Movie compareMovie, bool sortTitleSecondLayer = false){ //If sortYearSecondLayer, then filter by title (decending) if they have the same year
        // A null value means that this object is greater.
        if(compareMovie == null){
            return 1;
        }else{
            int compared = this.Year.CompareTo(compareMovie.Year);
            if(compared == 0){ //Same year
                if(sortTitleSecondLayer){ //Sort alphabetically by title
                    return CompareToTitle(compareMovie);
                }else{
                    return 0;
                }
            }else{ //Diffrent year
                return compared;
            }
        }
    }

    // Secondary comparer for Movie (tile alphabetically decending)
    public int CompareToTitle(Movie compareMovie, bool sortYearSecondLayer = false){ //If sortYearSecondLayer, then filter by year (decending) if they have the same title
        // A null value means that this object is greater.
        if(compareMovie == null){
            return 1;
        }else{
            int compared = this.Title.CompareTo(compareMovie.Title);
            if(compared == 0){ //Same title
                if(sortYearSecondLayer){ //Sort by year
                    return CompareToYear(compareMovie);
                }else{
                    return 0;
                }
            }else{ //Diffrent title
                return compared;
            }
        }
    }

    public static GENRES[] SortGenres(List<GENRES> toSortGenres){
        List<string> toSortAsStrings = new List<string>(){};
        GENRES[] convertedBackToENUM = new GENRES[toSortGenres.Count()];
        foreach(GENRES genre in toSortGenres){
            toSortAsStrings.Add(GenresEnumToString(genre));
        }
        toSortAsStrings.Sort();
        for(int i=0; i<convertedBackToENUM.Length; i++){
            convertedBackToENUM[i] = GetEnumFromString(toSortAsStrings[i]);
        }
        return convertedBackToENUM;
    }

    public static GENRES GetEnumFromString(string genreStr)
    {
        switch (genreStr)
        {
            case "Action": return GENRES.ACTION;
            case "Adventure": return GENRES.ADVENTURE;
            case "Animation": return GENRES.ANIMATION;
            case "Children's": return GENRES.CHILDRENS;
            case "Comedy": return GENRES.COMEDY;
            case "Crime": return GENRES.CRIME;
            case "Documentary": return GENRES.DOCUMENTARY;
            case "Drama": return GENRES.DRAMA;
            case "Fantasy": return GENRES.FANTASY;
            case "Film-Noir": return GENRES.FILM_NOIR;
            case "Horror": return GENRES.HORROR;
            case "Musical": return GENRES.MUSICAL;
            case "Mystery": return GENRES.MYSTERY;
            case "Romance": return GENRES.ROMANCE;
            case "Sci-Fi": return GENRES.SCI_FI;
            case "Thriller": return GENRES.THRILLER;
            case "War": return GENRES.WAR;
            case "Western": return GENRES.WESTERN;
            case "(no genres listed)": return GENRES.NO_GENRES_LISTED;
            default: return GENRES.ERROR_NOT_A_VALID_GENRE;
        }
    }
    public static string GenresEnumToString(GENRES genre)
    {
        switch (genre)
        {
            case GENRES.ACTION: return "Action";
            case GENRES.ADVENTURE: return "Adventure";
            case GENRES.ANIMATION: return "Animation";
            case GENRES.CHILDRENS: return "Children's";
            case GENRES.COMEDY: return "Comedy";
            case GENRES.CRIME: return "Crime";
            case GENRES.DOCUMENTARY: return "Documentary";
            case GENRES.DRAMA: return "Drama";
            case GENRES.FANTASY: return "Fantasy";
            case GENRES.FILM_NOIR: return "Film-Noir";
            case GENRES.HORROR: return "Horror";
            case GENRES.MUSICAL: return "Musical";
            case GENRES.MYSTERY: return "Mystery";
            case GENRES.ROMANCE: return "Romance";
            case GENRES.SCI_FI: return "Sci-Fi";
            case GENRES.THRILLER: return "Thriller";
            case GENRES.WAR: return "War";
            case GENRES.WESTERN: return "Western";
            case GENRES.NO_GENRES_LISTED: return "(no genres listed)";
            default: return "ERROR: NOT A VALID GENRE";
        }
    }
}