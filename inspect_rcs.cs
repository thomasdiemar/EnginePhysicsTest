using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        var dllPath = @"C:\Users\thoma\.nuget\packages\rcs\3.0.0\lib\net472\RCS.dll";
        try
        {
            var assembly = Assembly.LoadFrom(dllPath);
            var types = assembly.GetTypes();
            
            foreach (var type in types)
            {
                Console.WriteLine($"{type.FullName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
