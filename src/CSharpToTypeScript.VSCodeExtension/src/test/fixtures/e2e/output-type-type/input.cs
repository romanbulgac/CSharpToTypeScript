public class BaseDto
{
    public string Id { get; set; }
}

public class MyTypeDto : BaseDto
{
    public string Name { get; set; }
    public int Age { get; set; }
}
