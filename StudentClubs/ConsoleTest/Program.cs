using Core;

Club club = new Club {
    Name = "Гурток веб-розробки", Kind = "гурток",
    Faculty = "Факультет комп'ютерних наук та кібернетики",
    Day = "Середа", Time = "17:00", Course = "1", Topic = "Веб-розробка",
    Head = "Ткаченко Дарина Олегівна", Supervisor = "Савченко Андрій Петрович"
};

Console.WriteLine(club);
Console.WriteLine(club.ToXElement());
Console.WriteLine(club.ContainsKeyword("КОМП’ЮТЕРНИХ"));
Console.WriteLine(club.ContainsKeyword("фізика"));