using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        PrintAsciiArt();
        PrintColoredText();
        
        try
        {
            string gamePath = AskForGamePath();
            if (!string.IsNullOrEmpty(gamePath))
            {
                ModifyAssembly(gamePath);
            }
            else
            {
                Console.WriteLine("Invalid game path. Exiting...");
            }
        }
        catch (Exception ex)
        {
            LogException(ex, GetExecutableDirectory());
        }
    }
    
    static void PrintAsciiArt()
    {
        Console.ForegroundColor = ConsoleColor.Red; // Set color to red
        Console.WriteLine(
            @"
db    db d888888b d888888b  .o88b.        d8888b.  .d8b.  d888888b  .o88b. db   db d88888b d8888b. 
88    88   `88'   `~~88~~' d8P  Y8        88  `8D d8' `8b `~~88~~' d8P  Y8 88   88 88'     88  `8D 
88    88    88       88    8P             88oodD' 88ooo88    88    8P      88ooo88 88ooooo 88oobY' 
88    88    88       88    8b      C8888D 88~~~   88~~~88    88    8b      88~~~88 88~~~~~ 88`8b   
88b  d88   .88.      88    Y8b  d8        88      88   88    88    Y8b  d8 88   88 88.     88 `88. 
~Y8888P' Y888888P    YP     `Y88P'        88      YP   YP    YP     `Y88P' YP   YP Y88888P 88   YD"
        );
        Console.ResetColor(); // Reset color to default
    }
    
    static void PrintColoredText()
    {
        Console.ForegroundColor = ConsoleColor.Cyan; // Set color to cyan
        Console.Write("                                                                         v1.0.1");
        Console.ResetColor(); // Reset color to default
        Console.Write(" | by ");
        Console.ForegroundColor = ConsoleColor.Yellow; // Set color to yellow
        Console.WriteLine("Official-Husko");
        Console.ResetColor(); // Reset color to default
        Console.WriteLine();
    }
    
    static string AskForGamePath()
    {
        Console.WriteLine("Make sure to point it to your data folder! (e.g. C:\\Users\\paw_beans\\Downloads\\UiTC_v33b_EX_Win_64_Bit\\UiTC_v33b_EX_Win_64_Bit_Data)");
        Console.Write("Enter the path to the game data folder: ");
        string gamePath = Console.ReadLine();

        // Remove any trailing slashes or backslashes
        gamePath = gamePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (Directory.Exists(gamePath))
        {
            return gamePath;
        }
        else
        {
            Console.WriteLine("Invalid game path. Please make sure the directory exists.");
            return null;
        }
    }
    
    static void ModifyAssembly(string gamePath)
    {
        // Construct paths for the DLL and its backup
        string dllPath = Path.Combine(gamePath, @"Managed\Assembly-CSharp.dll");
        string backupFilePath = Path.Combine(gamePath, @"Managed\Assembly-CSharp-Backup.dll");

        // Load the assembly
        ModuleDefMD module = ModuleDefMD.Load(dllPath);

        // Find the type and the fields to modify
        TypeDef globalObjectsType = module.Types.SingleOrDefault(t => t.Name == "GlobalObjects_UW");

        if (globalObjectsType != null)
        {
            // Find the static constructor (.cctor) method
            MethodDef cctorMethod = globalObjectsType.Methods.SingleOrDefault(m => m.Name == ".cctor");

            if (cctorMethod != null)
            {
                Dictionary<string, string> fieldsWithTypes = new Dictionary<string, string>
                {
                    {"showDevDebug", "bool"},
                    {"isDevBuild", "bool"},
                    {"isDebugMode", "bool"},
                    {"ignoreChoiceReqs", "bool"},
                    {"overpowerStars", "bool"},
                    {"accessAllClothes", "bool"},
                    {"maxLuck", "bool"},
                    {"pregnancyInfoCheat", "bool"},
                    {"cheatsOn", "bool"},
                    {"isVersionExtended", "bool"},
                    {"enc", "int"},
                    {"showInternalDebug", "bool"},
                    {"isFurries", "bool"},
                    {"isSuperDebugging", "bool"},
                };

                List<string> youtubeLinks = new List<string>
                {
                    "https://www.youtube.com/watch?v=o9l4EiYFZjg",
                    "https://www.youtube.com/watch?v=egYUfUo3__k"
                };

                foreach (var kvp in fieldsWithTypes)
                {
                    string fieldName = kvp.Key;
                    string fieldType = kvp.Value;

                    FieldDef field = globalObjectsType.Fields.SingleOrDefault(f => f.Name == fieldName);

                    if (field != null)
                    {
                        if (fieldName == "enc")
                        {
                            continue;
                        }

                        object fieldValue = AskForInput(fieldName, fieldType);

                        for (int i = 0; i < cctorMethod.Body.Instructions.Count; i++)
                        {
                            Instruction instr = cctorMethod.Body.Instructions[i];
                            if (instr.OpCode == OpCodes.Ldc_I4_0 && i + 1 < cctorMethod.Body.Instructions.Count &&
                                cctorMethod.Body.Instructions[i + 1].OpCode == OpCodes.Stsfld &&
                                cctorMethod.Body.Instructions[i + 1].Operand == field)
                            {
                                instr.OpCode = fieldValue is bool boolValue && boolValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
                                break;
                            }
                            else if (instr.OpCode == OpCodes.Ldc_I4 && i + 1 < cctorMethod.Body.Instructions.Count &&
                                cctorMethod.Body.Instructions[i + 1].OpCode == OpCodes.Stsfld &&
                                cctorMethod.Body.Instructions[i + 1].Operand == field)
                            {
                                if (field.Constant is { } constantValue)
                                {
                                    instr.Operand = fieldValue;
                                }
                                else
                                {
                                    Console.WriteLine($"Invalid constant value for '{fieldName}'. Defaulting to false.");
                                    instr.Operand = 0;
                                }
                                break;
                            }
                        }

                        if (fieldName == "isVersionExtended" && fieldValue is bool isVersionExtended && isVersionExtended)
                        {
                            FieldDef encField = globalObjectsType.Fields.SingleOrDefault(f => f.Name == "enc");
                            if (encField != null)
                            {
                                int encValue = isVersionExtended ? 76 : 77;

                                for (int i = 0; i < cctorMethod.Body.Instructions.Count; i++)
                                {
                                    Instruction currentInstr = cctorMethod.Body.Instructions[i];
                                    if (currentInstr.OpCode == OpCodes.Ldc_I4 && i + 1 < cctorMethod.Body.Instructions.Count &&
                                        cctorMethod.Body.Instructions[i + 1].OpCode == OpCodes.Stsfld &&
                                        cctorMethod.Body.Instructions[i + 1].Operand == encField)
                                    {
                                        currentInstr.Operand = encValue;
                                    }
                                }
                            }
                        }

                        if (fieldName == "isFurries" && fieldValue is bool isFurries && isFurries)
                        {
                            OpenRandomYouTubeLink(youtubeLinks);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Field '{fieldName}' not found. The Game Dev removed it.");
                    }
                }

                File.Move(dllPath, backupFilePath);
                module.Write(dllPath);
            }
            else
            {
                Console.WriteLine("Static constructor (.cctor) not found.");
            }
        }
        else
        {
            Console.WriteLine("Type not found. Make sure the structure matches.");
        }
    }

    static object AskForInput(string fieldName, string fieldType)
    {
        try
        {
            Console.Write($"Enable {fieldName}? (y/n): ");

            if (fieldType == "bool")
            {
                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine();

                if (key.Key == ConsoleKey.Y)
                {
                    return true;
                }
                else if (key.Key == ConsoleKey.N)
                {
                    return false;
                }
                else
                {
                    Console.WriteLine("Invalid input. Defaulting to false.");
                    return false;
                }
            }
            else if (fieldType == "int")
            {
                return int.Parse(Console.ReadLine());
            }
            else
            {
                Console.WriteLine($"Unsupported field type: {fieldType}");
                return null;
            }
        }
        catch (Exception ex)
        {
            LogException(ex, GetExecutableDirectory());
            return null;
        }
    }

    static void OpenRandomYouTubeLink(List<string> links)
    {
        try
        {
            if (links.Count > 0)
            {
                Random random = new Random();
                int index = random.Next(links.Count);
                string randomLink = links[index];

                Process.Start(new ProcessStartInfo(randomLink) { UseShellExecute = true });
            }
            else
            {
                Console.WriteLine("No YouTube links available. :(");
            }
        }
        catch (Exception ex)
        {
            LogException(ex, GetExecutableDirectory());
        }
    }

    static void LogException(Exception ex, string directory)
    {
        try
        {
            string logFilePath = Path.Combine(directory, "runtime.log");
            File.WriteAllText(logFilePath, $"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
            Console.WriteLine($"An error occurred. Details logged in {logFilePath}. Please Report this!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch
        {
            Console.WriteLine("An error occurred, and failed to log details. Please Report this!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    static string GetExecutableDirectory()
    {
        return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
    }
}

/*
This code is probably the biggest dog shit out there but i don't care. it is what it is
*/