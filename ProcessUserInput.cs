using System.Data.SqlClient;


namespace FlashcardsApp
{
    public class ProcessUserInput
    {
        public static void AddOrModifyChosen(SqlConnection conn)
        {
            Console.WriteLine("Enter a to add a card, u to update a card, d to delete a card, or q to quit ");
            string choice = Console.ReadLine();
            string stackName;
            while(choice != "q")
            {
                if(choice == "a")
                {
                    Console.WriteLine("Enter stack name to add to (new stack will be created if not already existing): ");
                    stackName = Console.ReadLine();
                    Console.WriteLine("Enter the term you want to add and its definition separated by -- (2 dashes) :");
                    string[] card = Console.ReadLine().Split("--");
                    DatabaseManagement.AddCard(conn, stackName, card[0], card[1]);
                }
                else if(choice == "d")
                {
                    Console.WriteLine("Enter stack name to delete from: ");
                    stackName = Console.ReadLine();
                    Console.WriteLine("Enter the term you want to delete: ");
                    string term = Console.ReadLine();
                    DatabaseManagement.DeleteCard(conn, stackName, term);
                }
                else if(choice == "u")
                {
                    Console.WriteLine("Enter stack name to update in: ");
                    stackName = Console.ReadLine();
                    Console.WriteLine("Enter the term you want to update and its new definition separated by -- (2 dashes) :");
                    string[] card = Console.ReadLine().Split("--");
                    DatabaseManagement.AddCard(conn, stackName, card[0], card[1]);
                }
                
                if(choice != "q")
                {
                    Console.WriteLine("Enter a to add a card, u to update a card, d to delete a card, or q to quit ");
                    choice = Console.ReadLine();
                }
            }
        }

        public static async void StudySession(SqlConnection conn)
        {
            Console.WriteLine("Which stack would you like to study?");
            string stackName = Console.ReadLine();
            Console.WriteLine("How long do you want between the term and definition (seconds), an invalid value will be taken as 0:");
            int seconds;
            Int32.TryParse(Console.ReadLine(), out seconds);
            StackDTO stack = DatabaseManagement.GetCardsWithinStack(conn, stackName);
            if(stack == null) {
                Console.WriteLine("Stack does not exist");
                return; 
            }
            int cardNum = 0;
            string keepGoing = "c";
            int amountCorrect = 0; int amountWrong = 0;
            DateTime startTime = DateTime.Now;

            while(keepGoing == "c")
            {
                Card currCard = stack.CardsInStack.ElementAt(cardNum % stack.CardsInStack.Count());
                Console.WriteLine(currCard.Term);
                Thread.Sleep(seconds*1000);
                Console.WriteLine(currCard.Definition);
                Console.WriteLine("Enter y if you were correct and n if wrong, if something else is entered neither will be affected:");
                string grade = Console.ReadLine();
                if(grade == "y")
                {
                    amountCorrect++;
                }
                else if(grade == "n")
                {
                    amountWrong++;
                }
                Console.WriteLine("Enter c to continue, anything else to end the session:");
                keepGoing = Console.ReadLine();
            }
            int timePassed = (int)(DateTime.Now - startTime).TotalMinutes;
            Console.WriteLine("Your session has been logged.");
            DatabaseManagement.LogStudySession(conn, amountCorrect, amountWrong, stackName, timePassed);

        }

        public static void ViewStudySessions(SqlConnection conn)
        {
            List<StudySessionDTO> sessions = DatabaseManagement.GetSessions(conn);
            foreach(StudySessionDTO session in sessions)
            {
                Console.WriteLine("Stack studied: " + session.StackName);
                Console.WriteLine("Amount Correct: " + session.AmountCorrect + ", Amount Wrong: " + session.AmountWrong);
                Console.WriteLine("Time Spent: " + session.Minutes + "\n");
            }
        }
    }
}

