internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("메인 : 시작");

        Thread thread1 = new Thread(Work);

        thread1.Start();

        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"메인 : {i}");
        }
    }

    static void Work()
    {
        Console.WriteLine("쓰레드 : 시작");
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine($"쓰레드 : {i}");
        }
    }
}