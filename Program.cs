﻿using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using NLog;
using NLog.Fluent;

// Constents
bool IS_UNIX = true;
const string DELIMETER_1 = ",";
const string DELIMETER_2 = "|";



string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Movies), 
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Movies), 
                                        enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Exit)};


string loggerPath = Directory.GetCurrentDirectory() + (IS_UNIX? "/" : "\\") + "nlog.config";
string moviesPath = Directory.GetCurrentDirectory() + (IS_UNIX? "/" : "\\") + "movies.csv";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(loggerPath).GetCurrentClassLogger();

logger.Info("Main program is running and log mager is started, program is running on a "+(IS_UNIX? "" : "non-")+"unix-based device.");

string optionsSelector(string[] options){
    string userInput;
    int selectedNumber = -1;
    bool userInputWasImproper = true;
    do{
        Console.WriteLine("Please select an option from the following...");
        for(int i = 1; i <= options.Length; i++){
            Console.WriteLine("  "+i+") "+options[i-1]);
        }
        Console.Write("Please enter a option from the list: ");
        userInput = Console.ReadLine();

        //TODO: Move to switch without breaks instead of ifs or if-elses?
        if(!int.TryParse(userInput, out selectedNumber)){// User response was not a integer
            logger.Error("Your selector choice was not a integer, please try again.");
        }else if(selectedNumber < 1 || selectedNumber > options.Length){// User response was out of bounds
            logger.Error($"Your selector choice was not within bounds, please try again. (Range is 1-{options.Length})");
        }else{
            userInputWasImproper = false;
        }
    }while(userInputWasImproper);
    return options[selectedNumber-1];
}


while(true){

    // TODO: Move to switch with integer of place value and also make not relient on index by switching to enum for efficiency
    string menuCheckCommand = optionsSelector(MAIN_MENU_OPTIONS_IN_ORDER);

    if(menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Exit)){//If user intends to exit the program
        logger.Info("Program quiting...");
        return;
    }else if(menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.View_Movies)){
        displayMoviesFromFile(moviesPath);
    }else if(menuCheckCommand == enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS.Add_Movies)){

    }else{
        logger.Fatal("Somehow, menuCheckCommand was slected that did not fall under the the existing commands, this should never have been triggered. Improper menuCheckCommand is getting through");
    }
    
}

void displayMoviesFromFile(string dataPath){
    // ALL TERMINATORS
    if(!System.IO.File.Exists(dataPath)){
        logger.Fatal($"The file, '{dataPath}' was not found.");
        return;
    }
    // Take care of the rest of at this point all unknown filesystem errors (not accessable, ect.)
    StreamReader sr;
    try{
        sr = new StreamReader(dataPath);
    }catch(Exception ex){
        logger.Fatal(ex.Message);
        return;
    }
    while (!sr.EndOfStream){
        string line = sr.ReadLine();
        Console.WriteLine("line = "+line);
    }
    sr.Close();

//     // Caculate just once
//     string weekHeader = $" {DayOfWeek.Sunday.ToString().Substring(0,numPlacesInColumnAfterDividerDays),numPlacesInColumnAfterDividerDays} {DayOfWeek.Monday.ToString().Substring(0,numPlacesInColumnAfterDividerDays),numPlacesInColumnAfterDividerDays} {DayOfWeek.Tuesday.ToString().Substring(0,numPlacesInColumnAfterDividerDays),numPlacesInColumnAfterDividerDays} {DayOfWeek.Wednesday.ToString().Substring(0,numPlacesInColumnAfterDividerDays),numPlacesInColumnAfterDividerDays} {DayOfWeek.Thursday.ToString().Substring(0,numPlacesInColumnAfterDividerDays),numPlacesInColumnAfterDividerDays} {DayOfWeek.Friday.ToString().Substring(0,numPlacesInColumnAfterDividerDays),numPlacesInColumnAfterDividerDays} {DayOfWeek.Saturday.ToString().Substring(0,numPlacesInColumnAfterDividerDays),numPlacesInColumnAfterDividerDays}";
//     string calcHeader = $" {"Total".Substring(0,numPlacesInColumnAfterDividerCaculated),numPlacesInColumnAfterDividerCaculated} {"Avg".Substring(0,numPlacesInColumnAfterDividerCaculated),numPlacesInColumnAfterDividerCaculated}";
//     string underLineHeader = "";
//     for (int i = 0; i < 7; i++)
//     {
//         underLineHeader += " ";
//         for (int j = 0; j < numPlacesInColumnAfterDividerDays; j++)
//         {
//             underLineHeader += underLinePattern;
//         }
//     }
//     for (int i = 0; i < 2; i++)
//     {
//         underLineHeader += " ";
//         for (int j = 0; j < numPlacesInColumnAfterDividerCaculated; j++)
//         {
//             underLineHeader += underLinePattern;
//         }
//     }
//     string fullHeader = $"{weekHeader}{calcHeader}\n{underLineHeader}";

//     while (!sr.EndOfStream)
//     {
//         string line = sr.ReadLine();
//         DateTime date = DateTime.Parse(line.Substring(0, line.IndexOf(DELIMETER_1)));
//         Console.WriteLine($"Week of {date:MMM}, {date:dd}, {date:yyyy}");


//         string[] nightHours = line.Substring(line.IndexOf(DELIMETER_1) + 1).Split(DELIMETER_2);
//         Console.WriteLine(fullHeader);

//         //Days
//         foreach (string nightHour in nightHours)
//         {
//             Console.Write($" {nightHour,numPlacesInColumnAfterDividerDays}");
//         }
//         //Caculated
//         // float[] nightHoursNums = new float[nightHours.Length];
//         float totalWeekNightHours = 0f;
//         for (int i = 0; i < nightHours.Length; i++)
//         {
//             totalWeekNightHours += float.Parse(nightHours[i]);
//         }

//         string average = $"{(totalWeekNightHours / nightHours.Length).ToString():F10}";//TODO:Make to numPlacesInColumnAfterDividerCaculated
//         Console.Write($" {totalWeekNightHours.ToString(),numPlacesInColumnAfterDividerCaculated}");

//         if(average.Length > numPlacesInColumnAfterDividerCaculated){
//             average = average.Substring(0,numPlacesInColumnAfterDividerCaculated);
//         }

//         Console.Write($" {average,numPlacesInColumnAfterDividerCaculated}");
//         Console.WriteLine("\n");
//     }
//     sr.Close();
}





// vvv UNUM STUFF vvv
string enumToStringMainMenuWorkArround(MAIN_MENU_OPTIONS mainMenuEnum){

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