using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium;

namespace FoodyNoPOMTests
{
    public class FoodyTests
    {
        private readonly static string BaseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:85";
        private WebDriver driver;
        private Actions actions;
        private string? lastCreatedFoodName;
        private string? lastCreatedFoodDescribe;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("profile.password_manager_enable", false);
            chromeOptions.AddArgument("--disable-search-engine-choice-screen");

            driver = new ChromeDriver(chromeOptions);
            actions = new Actions(driver);
            driver.Navigate().GoToUrl(BaseUrl);
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            driver.Navigate().GoToUrl($"{BaseUrl}/User/Login");

            driver.FindElement(By.XPath("//input[@name='Username']")).SendKeys("petq");
            driver.FindElement(By.XPath("//input[@name='Password']")).SendKeys("123456");

            driver.FindElement(By.XPath("//button[@class='btn btn-primary btn-block fa-lg gradient-custom-2 mb-3']")).Click();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            driver.Quit();
            driver.Dispose();
        }

        [Test, Order(1)]
        public void AddFoodWithInvalidDataTest()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Food/Add");

            var foodNameInput = driver.FindElement(By.XPath("//input[@id='name']"));
            foodNameInput.Clear();
            foodNameInput.SendKeys("");

            var foodDescribeInput = driver.FindElement(By.XPath("//input[@id='description']"));
            foodDescribeInput.Clear();
            foodDescribeInput.SendKeys("");

            var addFoodButton = driver.FindElement(By.XPath("//button[@class='btn btn-primary btn-block fa-lg gradient-custom-2 mb-3']"));
            addFoodButton.Click();

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/Food/Add"), "User is redirect");

            var errorMessage = driver.FindElement(By.XPath("//div[@class='text-danger validation-summary-errors']//li")).Text;
            Assert.That(errorMessage, Is.EqualTo("Unable to add this food revue!"), "The error message is not as expected");
        }

        [Test, Order(2)]
        public void AddRandomFoodTest()
        {
            lastCreatedFoodName = GenerateRandomName();
            lastCreatedFoodDescribe = GenerateRandomFoodDescribe();

            driver.Navigate().GoToUrl($"{BaseUrl}/Food/Add");

            var foodNameInput = driver.FindElement(By.XPath("//input[@id='name']"));
            foodNameInput.Clear();
            foodNameInput.SendKeys(lastCreatedFoodName);

            var foodDescribeInput = driver.FindElement(By.XPath("//input[@id='description']"));
            foodDescribeInput.Clear();
            foodDescribeInput.SendKeys(lastCreatedFoodDescribe);

            var addFoodButton = driver.FindElement(By.XPath("//button[@class='btn btn-primary btn-block fa-lg gradient-custom-2 mb-3']"));
            addFoodButton.Click();

            var currentUrl = driver.Url;
            Assert.That(currentUrl, Is.EqualTo($"{BaseUrl}/"), "User is redirect");

            var formFoods = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']")).Last();
            actions.ScrollToElement(formFoods).Perform();

            var foodsCollection = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']"));
            var lastAddedFoodTitle = foodsCollection.Last().FindElement(By.XPath(".//h2")).Text;

            Assert.That(lastAddedFoodTitle, Is.EqualTo(lastCreatedFoodName), "The name is not as expected");
        }

        [Test, Order(3)]
        public void EditLastAddedFoodTest()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/");

            var formFoods = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']")).Last();
            actions.ScrollToElement(formFoods).Perform();

            var foodsCollection = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']"));
            var lastAddedFoodEditButton = foodsCollection.Last().FindElement(By.XPath(".//a[text()='Edit']"));
            lastAddedFoodEditButton.Click();

            var foodNameInput = driver.FindElement(By.XPath("//input[@id='name']"));
            foodNameInput.Clear();
            foodNameInput.SendKeys(lastCreatedFoodName + "Edited");

            var addFoodButton = driver.FindElement(By.XPath("//button[@class='btn btn-primary btn-block fa-lg gradient-custom-2 mb-3']"));
            addFoodButton.Click();

            formFoods = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']")).Last();
            actions.ScrollToElement(formFoods).Perform();

            foodsCollection = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']"));
            var lastAddedFoodTitle = foodsCollection.Last().FindElement(By.XPath(".//h2")).Text;

            Assert.That(lastAddedFoodTitle, Is.EqualTo(lastCreatedFoodName));
            Console.WriteLine("Options to edit is not fully implemented");
        }

        [Test, Order(4)]
        public void SearchForFoodTitleTest()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/");

            var searchInput = driver.FindElement(By.XPath("//input[@name='keyword']"));
            searchInput.SendKeys(lastCreatedFoodName);

            var searchButton = driver.FindElement(By.XPath("//button[@class='btn btn-primary rounded-pill mt-5 col-2']"));
            searchButton.Click();

            var searchedFoodForm = driver.FindElement(By.XPath("//div[@class='row gx-5 align-items-center']"));
            actions.ScrollToElement(searchedFoodForm).Perform();

            var searchedFoodTitle = driver.FindElement(By.XPath("//div[@class='col-lg-6 order-lg-1']//h2")).Text;
            Assert.That(searchedFoodTitle, Is.EqualTo(lastCreatedFoodName), "The title does not match");

        }

        [Test, Order(5)]
        public void DeleteLastAddedFoodTest()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/");

            var formFoods = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']")).Last();
            actions.ScrollToElement(formFoods).Perform();

            var foodsCollection = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']"));
            var countFoodsBeforeDelete = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']")).Count();
            var lastAddedFoodDeleteButton = foodsCollection.Last().FindElement(By.XPath(".//a[text()='Delete']"));
            lastAddedFoodDeleteButton.Click();

            var countFoodsAfterDelete = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']")).Count();

            Assert.That(countFoodsBeforeDelete - 1, Is.EqualTo(countFoodsAfterDelete), "The foods count is the same");

            formFoods = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']")).Last();
            actions.ScrollToElement(formFoods).Perform();

            foodsCollection = driver.FindElements(By.XPath("//div[@class='row gx-5 align-items-center']"));
            var lastFoodAfterDelete = foodsCollection.Last().FindElement(By.XPath(".//h2")).Text;

            Assert.That(lastFoodAfterDelete, Is.Not.EqualTo(lastCreatedFoodName), "The food was not deleted");
        }

        [Test, Order(6)]
        public void SearchForDeletedFoodTest()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/");

            var searchInput = driver.FindElement(By.XPath("//input[@name='keyword']"));
            searchInput.SendKeys(lastCreatedFoodName);

            var searchButton = driver.FindElement(By.XPath("//button[@class='btn btn-primary rounded-pill mt-5 col-2']"));
            searchButton.Click();

            var searchedFoodForm = driver.FindElement(By.XPath("//div[@class='row gx-5 align-items-center']"));
            actions.ScrollToElement(searchedFoodForm).Perform();

            var errorMessage = driver.FindElement(By.XPath("//div[@class='col-lg-6 order-lg-1']//h2")).Text;
            Assert.That(errorMessage, Is.EqualTo("There are no foods :("), "The error message is not as expected.");

            var addFoodButton = driver.FindElement(By.XPath("//a[text()='Add Food']"));

            Assert.That(addFoodButton, Is.Not.Null, "The button is not present");
        }

        public string GenerateRandomName()
        {
            var random = new Random();
            return "Name: " + random.Next(1000, 10000);
        }

        public string GenerateRandomFoodDescribe()
        {
            var random = new Random();
            return "Food: " + random.Next(1000, 10000);
        }
    }
}