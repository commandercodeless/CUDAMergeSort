using System;
using System.IO;

namespace UnitConverter
{
    class Program
    {


        static void Main(string[] args)
        {
            int fileSize = 0;
            string data;

            data = ReadFile(ref fileSize);  //Call the method which will read in the file and return the string

            string[,] conversions = new string[fileSize, 3];    //create an array with room for every row in the file to store three strings

            ProcessFile(ref fileSize, ref conversions, data);   //Call the method which will split the data string and put it into the array

            Input(fileSize, conversions);   //Call the method which will prompt the user to input data to be converted
        }

        static string ReadFile(ref int fileSize)
        {
            string fileName;

            string data = "";

            string lineFromFile;

            fileName = "convert.txt";

            StreamReader reader;

            try
            {
                reader = new StreamReader(fileName);    //Check for convert.txt in the bin/debug folder
            }
            catch       //catch occurs if convert.txt is not in bin/debug
            {
                fileName = "../../convert.txt"; 
                try
                {
                    reader = new StreamReader(fileName);    //Check for convert.txt in the same folder as the code
                }
                catch   //if catch occurs again, tell the user where to put convert.txt and the close the program
                {
                    Console.WriteLine("convert.txt not found in the code's directory or in bin/debug.  Please place convert.txt in either bin/debug or the same directory as the code and restart the program");
                    Console.ReadKey();
                    Environment.Exit(0);
                    reader = new StreamReader("");  //reader has to equal something after this case despite the environment.exit so that it is not possible to be an empty variable later in the code
                }
            }

            while (!reader.EndOfStream)
            {
                lineFromFile = reader.ReadLine();
                data = data + lineFromFile + "/";   //Each line of the file is added together, with "/" dividing it so that it can be split apart again later
                fileSize++;     //fileSize keeps track of how many conversions were stored in the file convert.txt
            }
            return data;
        }

        static void ProcessFile(ref int fileSize, ref string[,] conversions, string data)
        {
            string[] splitString;
            string[] splitdata;

            data = data.ToLower();
            splitdata = data.Split('/');    //split the file back into lines by splitting it at every "/"
            for (int j = 0; j < fileSize; j++ )
            {
                splitString = splitdata[j].Split(',');   //split each line into three
                for (int i = 0; i < 3; i++)
                {
                        try     //try catch to stop the program crashing if the row is empty
                        {
                            conversions[j, i] = splitString[i];  //input the strings into the array
                        }
                        catch
                        { }
                }
            }
        }

        static void Input(int fileSize, string[,] conversions)
        {
            string input;
            string[] splitString;


            while (fileSize > 0)    //fileSize will always be larger than 0, unless convert.txt is empty, so this part will continually loop
            {
                Console.WriteLine("Please input in the format: Value,StartUnits,ConvertUnits");
                input = Console.ReadLine();     // user inputs the value and units to be converted
                input = input.ToLower();

                splitString = input.Split(',');     //user input is split so that it can be compared to the array data

                if (Convert.ToDouble(splitString[0]) <= 0)      // program checks if the value entered is negative, and exits if it is
                    Environment.Exit(0);

                Conversion(splitString, conversions, fileSize);     //Call the function to perform the conversion from the user's input and display the result
            }
        }

        static void Conversion(string[] splitString, string[,] conversions, int fileSize)
        {
            double result;
            double value;
            int match = 0;  //Match checks if a match is found, so that if no match is found the system will alert the user

            for (int i = 0; i < fileSize; i++)
            {
                try     //try catch to stop the program crashing if the row of the array is empty
                {
                    if (CompareStrings(splitString[1],conversions[i, 0]) && CompareStrings(splitString[2],(conversions[i, 1])))   //Call a function to compare the input units to the current row in the array
                    {
                        match++;    //Increase match so that the program knows a match has been found
                        result = Convert.ToDouble(splitString[0]) * Convert.ToDouble(conversions[i, 2]);    //Calculate the value of the conversion
                        Console.WriteLine(splitString[0] + " " + conversions[i, 0] + "s is " + result + " " + conversions[i, 1] + "s (1 " + conversions[i, 0] + " is " + conversions[i, 2] + " " + conversions[i, 1] + "s)");   //Display the result of the conversion, and the rate of conversion
                    }
                    else
                        if (CompareStrings(splitString[2], conversions[i, 0]) && CompareStrings(splitString[1], (conversions[i, 1])))   //Call a function to compare the input units to the current row in the array backwards
                        {
                            match++;    //Increase match so that the program knows a match has been found
                            result = Convert.ToDouble(splitString[0]) / Convert.ToDouble(conversions[i, 2]);    //Calculate the value of the conversion
                            value = 1 / Convert.ToDouble(conversions[i, 2]);    //Find the conversion between the units
                            Console.WriteLine(splitString[0] + " " + conversions[i, 1] + "s is " + result + " " + conversions[i, 0] + "s (1 " + conversions[i, 0] + " is " + value + " " + conversions[i, 1] + "s)");   //Display the result of the conversion, and the rate of conversion
                        }
                }
                catch
                { }
            }

            if (match == 0)     //system alerts the user if no match was found
                Console.WriteLine("Units entered were not found to be compatable");
        }

        static bool CompareStrings(string input, string data)
        {
            if (input.Contains(data) || data.Contains(input))   //Checks if the input or the arraydata contain each other to account for spaces
                return true;
            else
                return false;
        }
    }
}
