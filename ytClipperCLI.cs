using System;
using System.Diagnostics;

namespace MyApp
{
    internal class Program
    {
        static void Main()
        {
                      
            // variables to be changed before running the program
            string ytdlpLocation = @"D:\PATH_programs\yt-dlp.exe";          // yt-dlp path      
            string downloadLocation = @"D:\Downloads";                      // defualt download location
                                                                            
                       
            Console.WriteLine("Enter timestamped video link");
            string videoLink = Console.ReadLine();

            // Extracting timestamp and clean link from videoLink
            int timeS = ExtractTime(videoLink);
            string actualLink = ExtractLink(videoLink);

  
            // Asking user if they want to change the download location 
            string DLanswer = ProgramProtectorForStringAnswers("Do you want to change the download location? Y/N", "Y", "N");

            // changing download location           
            if (DLanswer == "Y")
            {
                Console.WriteLine("Enter download location");
                downloadLocation = Console.ReadLine();
            }


            // Asking user for the length of the clip they want to create        
            int clipLength = ProgramProtectorForIntAnswers("Enter clip length (s)");


            // Asking user if they want the clip to be the clipLength before or after the timestamp (e.g. the 60s before or after 00:02:45)
            string lengthPrompt = $"Do you want to clip {clipLength} seconds forwards (f) or backwards (b) from {ConvertTime(timeS)}?";
            string lengthAnswer = ProgramProtectorForStringAnswers(lengthPrompt, "F", "B");


            // Set the value of the yt-dlp arguments
            string arguments = SetCommand(actualLink, lengthAnswer, "F", "B");

            // Execute yt-dlp with arguments
            ytdlp(ytdlpLocation, arguments);


            // Functions

            // protecting program from wrong input
            string ProgramProtectorForStringAnswers(string prompt, string string1, string string2)
            {
                // Protects the program by making sure input from a prompt is equal to either one of two strings (string1 and string2)
                string answer;

                do
                {
                    Console.WriteLine(prompt);
                    answer = Console.ReadLine().ToUpper();

                    if (answer != string1 && answer != string2)
                    {
                        Console.WriteLine($"Invalid input. Enter {string1} or {string2}");
                    }

                } while (answer != string1 && answer != string2);

                return answer;
            }

            int ProgramProtectorForIntAnswers(string prompt)
            {
                // Protects program if a non integer answer is entered when expecting an integer, by using Try.Parse

                bool ValidInput = false;
                int variable;

                do
                {
                    Console.WriteLine(prompt);
                    string input = Console.ReadLine();

                    if (!int.TryParse(input, out variable))
                    {
                        Console.WriteLine("{0} is not an integer", input);
                        ValidInput = false;
                    }
                    else
                        ValidInput = true;

                } while (!ValidInput);

                return variable;
            }

            // Dealing with strings
            int ExtractTime(string link)
            {
                // Extracts the time (s) from a timestamped youtube link

                int index = link.IndexOf("t=");
                string time = link.Substring(index + 2);

                int timeSeconds = int.Parse(time);
                return timeSeconds;
            }

            string ExtractLink(string link)
            {
                // Extracts clean link from inputted timestamped link

                string[] splits = link.Split(new string[] { "?si=" }, StringSplitOptions.None);
                string cleanLink = splits[0];

                return cleanLink;
            }

            string ConvertTime(int timeS)
            {
                // Converts time from seconds to HH:MM:SS

                int hours = timeS / 3600;
                int minutes = timeS / 60;
                int seconds = timeS % 60;

                string result = $"{hours:D2}:{minutes:D2}:{seconds:D2}";

                return result;
            }

            // yt-dlp functions
            string SetCommand(string pVideoLink, string pAnswer, string option1, string option2)
            {
                // sets the yt-dlp argument based on the prompt answer

                string fCommand = null;
                // Converting current time from seconds to HH:MM:SS
                string timeHMS = ConvertTime(timeS);

                if (pAnswer == option1)
                {
                    // timeS + clipLength
                    int fTimeS = timeS + clipLength;
                    string fTimeHMS = ConvertTime(fTimeS);

                    // time -to (time + clipLength) e.g. 00:01:20 -to (00:01:20 + 10s)
                    fCommand = $"--download-sections \"*{timeHMS} - {fTimeHMS}\" --force-keyframes-at-cut {pVideoLink} -o \"{downloadLocation}\\%(title)s.%(ext)s\"";
                }
                else if (pAnswer == option2)
                {
                    // timeS - clipLength
                    int bTimeS = timeS - clipLength;
                    string bTimeHMS = ConvertTime(bTimeS);

                    // (time - clipLength) -to time  e.g. (00:01:20 - 10s) -to 00:01:20
                    fCommand = $"--download-sections \"*{bTimeHMS} - {timeHMS}\" --force-keyframes-at-cut {pVideoLink} -o \"{downloadLocation}\\%(title)s.%(ext)s\"";
                }
                return fCommand;
            }

            void ytdlp(string programLocation, string pArguments)
            {
                // executes yt-dlp command, passing in the arguments, using the ProcessStartInfo class

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = programLocation,
                    Arguments = pArguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                    process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }

            }            
        }
    }
} 