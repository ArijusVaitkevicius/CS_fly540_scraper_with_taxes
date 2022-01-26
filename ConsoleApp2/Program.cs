using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text;
using System.Text.RegularExpressions;

var scraper = new Fly540Scraper();

List<DateTime> firstDates = new List<DateTime>() { DateTime.Today.AddDays(10), DateTime.Today.AddDays(17) };
List<DateTime> secondDates = new List<DateTime>() { DateTime.Today.AddDays(20), DateTime.Today.AddDays(27) };
List<List<DateTime>> dates = new List<List<DateTime>>() { firstDates, secondDates };
string departureIATA = "NBO";
string arrivalIATA = "MBA";
string currency = "USD";

List<string> URLS = scraper.Urls(dates, departureIATA, arrivalIATA, currency);
scraper.FlightsScraper(URLS);

class Fly540Scraper
{
    public List<string> Urls(List<List<DateTime>> dates, string depIATA, string arrIATA, string cur)
    {
        List<string> URLS = new List<string>();

        //This loop generates URL by list variables.
        foreach (var date in dates)
        {
            string URL = $"https://www.fly540.com/flights/?isoneway=0&currency={cur}&depairportcode={depIATA}&arrvairportcode={arrIATA}&date_from={date[0].ToString("ddd")}%2C+{date[0].ToString("dd")}+{date[0].ToString("MMM")}+{date[0].ToString("yyy")}&date_to={date[1].ToString("ddd")}%2C+{date[1].ToString("dd")}+{date[1].ToString("MMM")}+{date[1].ToString("yyy")}&adult_no=1&children_no=0&infant_no=0&searchFlight=&change_flight=";
            URLS.Add(URL);
        }

        return URLS;

    }
    public void FlightsScraper(List<string> URLS)
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");
        IWebDriver driver = new ChromeDriver("c:/users/arijus/source/repos/consoleapp1/consoleapp1/bin/debug", chromeOptions);//Specify the path to ChromeDriver.exe
        driver.Manage().Window.Maximize();

        HtmlWeb web = new HtmlWeb();
        string reYear = @"\d{4}";
        var csv = new StringBuilder();
        string csvPATH = "C:/Users/Arijus/source/repos/ConsoleApp2/ConsoleApp2/result.csv"; //Specify the path to csv file

        //This loop iterates through URLS list and gets each URL page source.
        foreach (string url in URLS)
        {
            driver.Navigate().GoToUrl(url);
            Thread.Sleep(2000);
            var outCards = driver.FindElements(By.XPath("//div[@class='fly5-flights fly5-depart th']/div[@class='fly5-results']/div")).Count;
            var inCards = driver.FindElements(By.XPath("//div[@class='fly5-flights fly5-return th']/div[@class='fly5-results']/div")).Count;

            HtmlDocument document = web.Load(url);
            string outDate = document.DocumentNode.SelectSingleNode("//span[contains(text(), 'Departing')]").NextSibling.InnerText;
            string outYear = Regex.Matches(outDate, reYear)[0].Groups[0].Value;
            string inDate = document.DocumentNode.SelectSingleNode("//span[contains(text(), 'Returning')]").NextSibling.InnerText;
            string inYear = Regex.Matches(inDate, reYear)[0].Groups[0].Value;

            foreach (int outIdx in Enumerable.Range(0, outCards))
            {
                foreach (int inIdx in Enumerable.Range(0, inCards))
                {
                    var outbound = driver.FindElements(By.XPath("//div[@class='fly5-flights fly5-depart th']/div[@class='fly5-results']/div"))[outIdx];
                    var inbound = driver.FindElements(By.XPath("//div[@class='fly5-flights fly5-return th']/div[@class='fly5-results']/div"))[inIdx];


                    outbound.FindElement(By.XPath(".//span[@class='comparebtn']")).Click();
                    Thread.Sleep(2000);
                    var outSelectBtn = outbound.FindElement(By.XPath(".//button"));
                    outSelectBtn.Click();
                    Thread.Sleep(2000);
                    inbound.FindElement(By.XPath(".//span[@class='comparebtn']")).Click();
                    Thread.Sleep(2000);
                    var inSelectBtn = inbound.FindElement(By.XPath(".//button"));
                    inSelectBtn.Click();
                    Thread.Sleep(2000);
                    var continueBtn = driver.FindElement(By.XPath("//button[@id='continue-btn']"));
                    continueBtn.Click();
                    Thread.Sleep(2000);

                    HtmlDocument newDocument = web.Load(driver.Url);


                    //outbound details (outbound departure and arrival airports codes, price, dates and times).

                    var outCol = newDocument.DocumentNode.SelectSingleNode(".//div[contains(@class, 'fly5-fout')]");

                    string outFromIATA = outCol.SelectSingleNode(".//div[contains(@class, 'fly5-frshort')]").InnerText.Trim();

                    string outToIATA = outCol.SelectSingleNode(".//div[contains(@class, 'fly5-toshort')]").InnerText.Trim();

                    string outDepDate = outCol.SelectSingleNode(".//div[contains(@class, 'fly5-timeout')]/span[@class='fly5-fdate']").InnerText.Trim();

                    string outDepTime = outCol.SelectSingleNode(".//div[contains(@class, 'fly5-timeout')]/span[@class='fly5-ftime']").InnerText.Trim();

                    string fullOutDepDate = DateTime.ParseExact($"{outYear} {outDepDate} {outDepTime}", "yyyy ddd dd, MMM h:mmtt", null).ToString("ddd MMM dd HH:mm:ss 'GMT' yyyy");

                    string outArrDate = outCol.SelectSingleNode(".//div[contains(@class, 'fly5-timein')]/span[@class='fly5-fdate']").InnerText.Trim();

                    string outArrTime = outCol.SelectSingleNode(".//div[contains(@class, 'fly5-timein')]/span[@class='fly5-ftime']").InnerText.Trim();

                    string fullOutArrDate = DateTime.ParseExact($"{outYear} {outArrDate} {outArrTime}", "yyyy ddd dd, MMM h:mmtt", null).ToString("ddd MMM dd HH:mm:ss 'GMT' yyyy");


                    ////Inbound details(inbound departure and arrival airports codes, price, dates and times).
                    var inCol = newDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'fly5-fin')]");


                    string inFromIATA = inCol.SelectSingleNode(".//div[contains(@class, 'fly5-frshort')]").InnerText.Trim();

                    string inToIATA = inCol.SelectSingleNode(".//div[contains(@class, 'fly5-toshort')]").InnerText.Trim();

                    string inDepDate = inCol.SelectSingleNode(".//div[contains(@class, 'fly5-timeout')]/span[@class='fly5-fdate']").InnerText.Trim();

                    string inDepTime = inCol.SelectSingleNode(".//div[contains(@class, 'fly5-timeout')]/span[@class='fly5-ftime']").InnerText.Trim();

                    string fullInDepDate = DateTime.ParseExact($"{inYear} {inDepDate} {inDepTime}", "yyyy ddd dd, MMM h:mmtt", null).ToString("ddd MMM dd HH:mm:ss 'GMT' yyyy");

                    string inArrDate = inCol.SelectSingleNode(".//div[contains(@class, 'fly5-timein')]/span[@class='fly5-fdate']").InnerText.Trim();

                    string inArrTime = inCol.SelectSingleNode(".//div[contains(@class, 'fly5-timein')]/span[@class='fly5-ftime']").InnerText.Trim();

                    string fullInArrDate = DateTime.ParseExact($"{inYear} {inArrDate} {inArrTime}", "yyyy ddd dd, MMM h:mmtt", null).ToString("ddd MMM dd HH:mm:ss 'GMT' yyyy");

                    string price = newDocument.DocumentNode.SelectSingleNode("//span[@class='fly5-price']").InnerText.Trim();

                    var taxes = newDocument.DocumentNode.SelectNodes("//div[contains(text(), 'Tax')]/span");

                    double finalTaxes = 0;

                    foreach (var tax in taxes)
                    {
                        finalTaxes += Convert.ToDouble(tax.InnerText.Trim());
                    }

                    var newLine = $"{outFromIATA};{outToIATA};{fullOutDepDate};{fullOutArrDate};{inFromIATA};{inToIATA};{fullInDepDate};{fullInArrDate};{price};{finalTaxes.ToString("F" + 2)}";

                    Console.WriteLine(newLine);
                    //Adds line to csv variable.
                    csv.AppendLine(newLine);

                    driver.Navigate().Back();
                    Thread.Sleep(2000);
                }
            }
        }
        driver.Close();


        if (!File.Exists(csvPATH))
        {
            string header = $"outbound_departure_airport;outbound_arrival_airport;outbound_departure_time;outbound_arrival_time;inbound_departure_airport;inbound_arrival_airport;inbound_departure_time;inbound_arrival_time;total_price;taxes{Environment.NewLine}";
            File.WriteAllText(csvPATH, header);
        }

        File.AppendAllText(csvPATH, csv.ToString());
    }
}
