using Core;

SearchCriteria criteria = new SearchCriteria();
criteria.Set("Faculty", "Факультет комп'ютерних наук та кібернетики");
criteria.Set("Course", "2");
criteria.Set("Day", "");
criteria.Keyword = "логік";

Console.WriteLine(criteria);
Console.WriteLine(criteria.Active().Count);
Console.WriteLine(criteria.Get("Day") == "");

try {
    criteria.Set("Room", "214");
} catch (ArgumentException ex) {
    Console.WriteLine(ex.Message);
}

criteria.Clear();
Console.WriteLine(criteria.IsEmpty());
Console.WriteLine(criteria);