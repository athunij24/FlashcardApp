using System.Data.SqlClient;
using System.Configuration;


namespace FlashcardsApp
{
    public class DatabaseManagement
    {

        public static void DatabaseSetup()
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.AppSettings["ConnectionString"]))
            {
                conn.Open();
                
                string checkTablesQuery = @"SELECT COUNT(*) FROM sys.tables WHERE[type] = 'U';";
                SqlCommand cmd = new SqlCommand(checkTablesQuery, conn);
                int tableCount = (int)cmd.ExecuteScalar();
                if(tableCount == 3) { return; }

                string createStacksTableQuery = @"
                CREATE TABLE Stacks (
                    StackId int IDENTITY(1,1) PRIMARY KEY,
                    StackName NVARCHAR(MAX) NOT NULL
                );";
                SqlCommand createTable1 = new SqlCommand(createStacksTableQuery, conn);
                createTable1.ExecuteNonQuery();

                string createCardsTableQuery = @"
                CREATE TABLE Cards (
                    StackId int NOT NULL,
                    Id int IDENTITY PRIMARY KEY,
                    Term NVARCHAR(MAX),
                    Definition NVARCHAR(MAX),
                    FOREIGN KEY (StackId) REFERENCES Stacks(StackId)
                );";
                SqlCommand createTable2 = new SqlCommand(createCardsTableQuery, conn);
                createTable2.ExecuteNonQuery();

                string createSessionsTableQuery = @"
                CREATE TABLE Sessions (
                    StackName NVARCHAR(MAX),
                    Id int IDENTITY PRIMARY KEY,
                    AmountCorrect int,
                    AmountWrong int,
                    Minutes int
                );";
                SqlCommand createTable3 = new SqlCommand(createSessionsTableQuery, conn);
                createTable3.ExecuteNonQuery();
                //debug statement
                tableCount = (int)cmd.ExecuteScalar();
            }
            
        }

        public static SqlConnection ConnectToDB()
        {
            string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        public static string AddCard(SqlConnection conn, string stackName, string term, string definition)
        {   
            int stackID = GetStack(conn,stackName);
            string stackSQL = @"
                DECLARE @InsertedIds TABLE (Id int);
                INSERT INTO Stacks (StackName)
                OUTPUT inserted.StackId INTO @InsertedIds
                VALUES (@STACKNAME);
                SELECT Id FROM @InsertedIds;";
            if (stackID == -1) {
                using (SqlCommand createStack = new SqlCommand(stackSQL,conn))
                {
                    createStack.Parameters.AddWithValue("@STACKNAME", stackName);
                    stackID = (int)createStack.ExecuteScalar();
                }
            }

            if(!CardExistsInStack(conn, stackID, term))
            {
                using (SqlCommand insertCard = new SqlCommand("INSERT INTO Cards (StackId, Term, Definition) VALUES (@STACKID, @TERM, @DEFINITION)", conn))
                {
                    insertCard.Parameters.AddWithValue("@STACKID", stackID);
                    insertCard.Parameters.AddWithValue("@TERM", term);
                    insertCard.Parameters.AddWithValue("@DEFINITION", definition);
                    if(insertCard.ExecuteNonQuery() == 1)
                    {
                        return "Succesfully added flashcard";
                    }
                    else
                    {
                        return "Could not add flashcard";
                    }
                }
            }
            else
            {
                return "Card exists in this stack with this term, update it instead";
            }
        }

        public static string DeleteCard(SqlConnection conn, string stackName, string term)
        {
            int stackID = GetStack(conn, stackName);
            if (stackID == -1)
            {
                return "Cannot delete card from nonexistent stack";
            }

            using (SqlCommand deleteCard = new SqlCommand("DELETE FROM Cards WHERE StackId = @STACKID AND Term = @TERM", conn))
            {
                deleteCard.Parameters.AddWithValue("@STACKID", stackID);
                deleteCard.Parameters.AddWithValue("@TERM", term);
                if (deleteCard.ExecuteNonQuery() == 1)
                {
                    return "Succesfully deleted card from stack";
                }
                else { return "Not able to delete the card"; }
            }
        }

        public static string UpdateCard(SqlConnection conn, string stackName, string term, string definition)
        {
            int stackID = GetStack(conn, stackName);
            if (stackID == -1)
            {
                return "Cannot update card from nonexistent stack";
            }

            using (SqlCommand updateCard = new SqlCommand("UPDATE Cards SET Definition = @DEF WHERE Term = @TERM", conn))
            {
                updateCard.Parameters.AddWithValue("@DEF",definition);
                updateCard.Parameters.AddWithValue("@TERM", term);
                if(updateCard.ExecuteNonQuery() == 1)
                {
                    return "Successfully updated the card's definition";
                }
                else
                {
                    return "Card did not exist or definition was the same";
                }
            }
        }

        public static StackDTO GetCardsWithinStack(SqlConnection conn, string stackName)
        {
            int stackID = GetStack(conn, stackName);
            if (stackID == -1)
            {
                return null;
            }
            Stack stack = new Stack{
                Name = stackName,
                CardsInStack = new List<Card>()
            };
            stack.Name = stackName;
            using (SqlCommand getCards = new SqlCommand("SELECT * from Cards WHERE StackId = @STACKID", conn)){
                getCards.Parameters.AddWithValue("@STACKID", stackID);
                using (SqlDataReader reader = getCards.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Card card = new Card(reader["Term"].ToString(), reader["Definition"].ToString());
                        stack.CardsInStack.Add(card);
                    }
                }
            }
            return StackMapper.MapToDTO(stack);
        }

        public static int GetStack(SqlConnection conn, string stackName)
        {
            using (SqlCommand getStack = new SqlCommand("SELECT * from Stacks WHERE StackName = @STACKNAME", conn))
            {
                getStack.Parameters.AddWithValue("@STACKNAME", stackName);
                using (SqlDataReader reader = getStack.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var stackID = reader["StackId"];
                        return (int)stackID;

                    }
                    else
                    {
                        return -1;
                    }
                }
            }
        }

        public static bool CardExistsInStack(SqlConnection conn, int stackID, string term)
        {
            using (SqlCommand sqlCommand = new SqlCommand("SELECT * from Cards WHERE StackId = @STACKID", conn))
            {
                sqlCommand.Parameters.AddWithValue("@STACKID", stackID);
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Term"].ToString() == term)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

        }

        public static void LogStudySession(SqlConnection conn, int correct, int wrong, string stackName, int minutes)
        {
            using (SqlCommand sqlCommand = new SqlCommand("INSERT INTO Sessions (StackName, AmountCorrect, AmountWrong, Minutes) VALUES (@STACKNAME, @AMOUNTCORRECT, @AMOUNTWRONG, @MINUTES)", conn))
            {
                sqlCommand.Parameters.AddWithValue("@STACKNAME", stackName);
                sqlCommand.Parameters.AddWithValue("@AMOUNTCORRECT", correct);
                sqlCommand.Parameters.AddWithValue("@AMOUNTWRONG", wrong);
                sqlCommand.Parameters.AddWithValue("@MINUTES", minutes);
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static List<StudySessionDTO> GetSessions(SqlConnection conn)
        {
            List<StudySessionDTO> studySessions = new List<StudySessionDTO>();
            using (SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Sessions", conn))
            {
                using(SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StudySession currSession = new StudySession
                        {
                            Id = (int)reader["Id"],
                            StackName = reader["StackName"].ToString(),
                            AmountCorrect = (int)reader["AmountCorrect"],
                            AmountWrong = (int)reader["AmountWrong"],
                            Minutes = (int)reader["Minutes"]
                        };
                        studySessions.Add(SessionMapper.MapToSession(currSession));
                    }
                    return studySessions;
                }
            }
        }
        
    }
}
