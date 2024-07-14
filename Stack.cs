namespace FlashcardsApp
{
    public class Stack
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Card> CardsInStack { get; set; }
    }

    public class Card
    {
        public string Term { get; set; }
        public string Definition { get; set; }

        public Card(string term, string definition) {
            Term = term;
            Definition = definition;
        }
    }

    public class StackDTO
    {
        public string Name { get; }
        public List<Card> CardsInStack { get; }

        public StackDTO(string name, List<Card> cardsInStack)
        {
            Name = name;
            CardsInStack = cardsInStack;
        }
    }

    public static class StackMapper
    {
        public static StackDTO MapToDTO(Stack stack)
        {
            if (stack == null)
            {
                throw new ArgumentNullException(nameof(stack));
            }

            return new StackDTO(
                stack.Name,
                stack.CardsInStack
            );
        }
    }

    public class StudySession
    {
        public int Id { get; set; }
        public string StackName { get; set; }
        public int AmountCorrect { get; set; }
        public int AmountWrong { get; set; }
        public int Minutes { get; set; }
    }

    public class StudySessionDTO
    {
        public string StackName{ get; set; }
        public int AmountCorrect { get; set; }
        public int AmountWrong { get; set; }
        public int Minutes { get; set; }
    }

    public static class SessionMapper
    {
        public static StudySessionDTO MapToSession(StudySession session)
        {
            if(session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            return new StudySessionDTO
            {
                StackName = session.StackName,
                AmountCorrect = session.AmountCorrect,
                AmountWrong = session.AmountWrong,
                Minutes = session.Minutes
            };
        }
    }
}
