using System.Data.SqlClient;
namespace FlashcardsApp
{
    class Program
    {
        public static string promptUser()
        {
            Console.WriteLine("Select if you want to add cards or study:");
            Console.WriteLine("1. Add/Modify Cards" + "\n" + "2. Study" + "\n" + "3. View sessions" + "\n" + "4. Exit");
            string choice = Console.ReadLine();
            if (choice == "1" ||  choice == "2" || choice == "3" || choice == "4")
            {
                return choice;
            }
            else
            {
                Console.WriteLine("Choose one of the four options");
                return promptUser();
            }
        }
        static void Main(string[] args)
        {
            DatabaseManagement.DatabaseSetup();
            SqlConnection conn = DatabaseManagement.ConnectToDB();
            string userChoice = promptUser();
            do
            {
                switch (userChoice)
                {
                    case "1":
                        ProcessUserInput.AddOrModifyChosen(conn);
                        break;
                    case "2":
                        ProcessUserInput.StudySession(conn);
                        break;
                    case "3":
                        ProcessUserInput.ViewStudySessions(conn);
                        break;
                    case "4":
                        return;
                    default:
                        break;
                }
                userChoice = promptUser();

            } while (userChoice != "4");

        }
    }
}
