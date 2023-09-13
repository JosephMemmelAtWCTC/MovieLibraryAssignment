public class Movie : IEquatable<Movie>
{
    //TODO: Make programatic in future versions
    public const bool CONVERT_GREGORIAN_TO_HUMAN_ERA = false;
    public const byte YEAR_SPACE_FOR_DIGIT_PLACES = CONVERT_GREGORIAN_TO_HUMAN_ERA? 5 : 4;
    public const bool ALL_INPUT_YEARS_ARE_HUMAN_ERA = false;

    public int id { get; }// Can add set later if editing answer is implemented

    public string title { get; }

    public int year{ get; }//Allowed to be negative in case of using other year system other then gregorian

    public string[] genres{ get; }

    public Movie(int id, string title, /*params*/ string[] genres){//Cannot have "params" with overloaded method also uses strings there
        this.id = id;
        title = title.Trim();
        //If title follows specifc format "______(####)", seperate out year
        short lastOpeningParenthesieTitleIndex = (short)title.LastIndexOf("(");
        short lastClosingParenthesieTitleIndex = (short)title.LastIndexOf(")");
        // TODO: Decide to throw error if date is not included, it is not a field and decided as optional, not throwing/logging error for now
        if(lastClosingParenthesieTitleIndex == title.Length-1 //Closing parenthesies must be at the end of string, also checks for Last is ")...(" instead of "(...)", determine not including a title
           && lastClosingParenthesieTitleIndex - lastOpeningParenthesieTitleIndex-2 > 1 && lastClosingParenthesieTitleIndex - lastOpeningParenthesieTitleIndex - 2 <= YEAR_SPACE_FOR_DIGIT_PLACES //Number of charaters taking up must be at least 1, max YEAR_SPACE_FOR_DIGIT_PLACES, else determine does not include a title
           && short.TryParse(title.Substring(lastOpeningParenthesieTitleIndex+1,lastClosingParenthesieTitleIndex-lastOpeningParenthesieTitleIndex-1), out short year)
           ){
            this.title = title.Substring(0,title.Length-(title.Length-lastOpeningParenthesieTitleIndex)).TrimEnd();//Beginning has already been trimmed, only need to check the end
            if(CONVERT_GREGORIAN_TO_HUMAN_ERA && !ALL_INPUT_YEARS_ARE_HUMAN_ERA && year < 10_000){//Assumes that input is gregorian, unless year is at least 10,000
                this.year = 10_000 + year;
            }else{
                this.year = year;
            }
        }else{//Particular title does contain year, skip removal from title
            this.title = title;
            // this.year = 0;
        }
        this.genres = genres;
    }
    public Movie(int id, string title, string genreStringList, string genreStringDelimiter)
        : this(id, title, genreStringList.Split(genreStringDelimiter))
    {}

    public override string ToString()
    {
        return "Id: " + id + " Title: " + title + " Year: " + year;
    }
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Movie objAsMovie = obj as Movie;
        if (objAsMovie == null) return false;
        else return Equals(objAsMovie);
    }
    public override int GetHashCode()
    {
        return id.GetHashCode();
    }
    public bool Equals(Movie other)
    {
        if (other == null) return false;
        return (this.id.Equals(other.id));
    }
    // Should also override == and != operators.
}