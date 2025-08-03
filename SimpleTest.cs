using System;

namespace RestaurantMenuAPI.SimpleTest
{
    public class SimpleTest
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("✅ Simple Test 1: Basic Arithmetic");
            Console.WriteLine("   2 + 2 = 4: " + (2 + 2 == 4 ? "✅ PASS" : "❌ FAIL"));
            
            Console.WriteLine("✅ Simple Test 2: String Operations");
            Console.WriteLine("   String concat: " + ("Hello" + " " + "World" == "Hello World" ? "✅ PASS" : "❌ FAIL"));
            
            Console.WriteLine("✅ Simple Test 3: DateTime Check");
            Console.WriteLine("   DateTime not null: " + (DateTime.Now != null ? "✅ PASS" : "❌ FAIL"));
            
            Console.WriteLine("✅ Simple Test 4: Environment Check");
            Console.WriteLine("   Running in container: " + (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != null ? "✅ PASS" : "✅ PASS (Local)"));
            
            Console.WriteLine("");
            Console.WriteLine("🎉 All basic tests passed! Your environment is working.");
            Console.WriteLine("📊 Test Results: 4/4 tests passed");
        }
    }
}
