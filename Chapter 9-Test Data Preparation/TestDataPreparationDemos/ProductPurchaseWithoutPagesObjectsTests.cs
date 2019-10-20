﻿using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System.Threading;

namespace TestDataPreparationDemos
{
    [TestClass]
    public class ProductPurchaseWithoutPagesObjectsTests
    {
        private static Driver _driver;
        private static string _purchaseEmail;
        private static string _purchaseOrderNumber;
        private static Stopwatch _stopWatch;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _stopWatch = Stopwatch.StartNew();

            _driver = new LoggingDriver(new WebDriver());
            _driver.Start(Browser.Chrome);

            Debug.WriteLine($"End Browser Initialize: {_stopWatch.Elapsed.TotalSeconds}");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _driver.Quit();
            Debug.WriteLine(_stopWatch.Elapsed.TotalSeconds);
            _stopWatch.Stop();
        }

        [TestMethod]
        public void CompletePurchaseSuccessfully_WhenNewClient()
        {
            Debug.WriteLine($"Start CompletePurchaseSuccessfully_WhenNewClient: {_stopWatch.Elapsed.TotalSeconds}");
            AddRocketToShoppingCart();

            ApplyCoupon();

            IncreaseProductQuantity();

            var proceedToCheckout = _driver.FindElement(By.CssSelector("[class*='checkout-button button alt wc-forward']"));
            proceedToCheckout.Click();

            var billingFirstName = _driver.FindElement(By.Id("billing_first_name"));
            billingFirstName.TypeText("Anton");
            var billingLastName = _driver.FindElement(By.Id("billing_last_name"));
            billingLastName.TypeText("Angelov");
            var billingCompany = _driver.FindElement(By.Id("billing_company"));
            billingCompany.TypeText("Space Flowers");
            var billingCountryWrapper = _driver.FindElement(By.Id("select2-billing_country-container"));
            billingCountryWrapper.Click();
            var billingCountryFilter = _driver.FindElement(By.ClassName("select2-search__field"));
            billingCountryFilter.TypeText("Germany");

            var germanyOption = _driver.FindElement(By.XPath("//*[contains(text(),'Germany')]"));
            germanyOption.Click();

            var billingAddress1 = _driver.FindElement(By.Id("billing_address_1"));
            billingAddress1.TypeText("1 Willi Brandt Avenue Tiergarten");
            var billingAddress2 = _driver.FindElement(By.Id("billing_address_2"));
            billingAddress2.TypeText("Lützowplatz 17");
            var billingCity = _driver.FindElement(By.Id("billing_city"));
            billingCity.TypeText("Berlin");
            var billingZip = _driver.FindElement(By.Id("billing_postcode"));
            billingZip.TypeText("10115");
            var billingPhone = _driver.FindElement(By.Id("billing_phone"));
            billingPhone.TypeText("+00498888999281");
            var billingEmail = _driver.FindElement(By.Id("billing_email"));
            billingEmail.TypeText("info@berlinspaceflowers.com");
            _purchaseEmail = GenerateUniqueEmail();
            var createAccountCheckBox = _driver.FindElement(By.Id("createaccount"));
            createAccountCheckBox.Click();
            var checkPaymentsRadioButton = _driver.FindElement(By.CssSelector("[for*='payment_method_cheque']"));
            checkPaymentsRadioButton.Click();
            var placeOrderButton = _driver.FindElement(By.Id("place_order"));
            placeOrderButton.Click();

            var receivedMessage = _driver.FindElement(By.XPath("//h1"));
            Assert.AreEqual("Order received", receivedMessage.Text);

            Debug.WriteLine($"End CompletePurchaseSuccessfully_WhenNewClient: {_stopWatch.Elapsed.TotalSeconds}");
        }

        [TestMethod]
        public void CompletePurchaseSuccessfully_WhenExistingClient()
        {
            Debug.WriteLine($"Start CompletePurchaseSuccessfully_WhenExistingClient: {_stopWatch.Elapsed.TotalSeconds}");
            AddRocketToShoppingCart();
            ApplyCoupon();
            IncreaseProductQuantity();

            var proceedToCheckout = _driver.FindElement(By.CssSelector("[class*='checkout-button button alt wc-forward']"));
            proceedToCheckout.Click();

            var loginHereLink = _driver.FindElement(By.LinkText("Click here to login"));
            loginHereLink.Click();
            Login("info@berlinspaceflowers.com");

            var placeOrderButton = _driver.FindElement(By.Id("place_order"));
            placeOrderButton.Click();

            Thread.Sleep(4000);
            var receivedMessage = _driver.FindElement(By.XPath("//h1"));
            Assert.AreEqual("Order received", receivedMessage.Text);

            var orderNumber = _driver.FindElement(By.XPath("//*[@id='post-7']/div/div/div/ul/li[1]/strong"));
            _purchaseOrderNumber = orderNumber.Text;

            Debug.WriteLine($"End CompletePurchaseSuccessfully_WhenExistingClient: {_stopWatch.Elapsed.TotalSeconds}");
        }

        private void Login(string userName)
        {
            var userNameTextField = _driver.FindElement(By.Id("username"));
            userNameTextField.TypeText(userName);
            var passwordField = _driver.FindElement(By.Id("password"));
            passwordField.TypeText(GetUserPasswordFromDb(userName));
            var loginButton = _driver.FindElement(By.XPath("//button[@name='login']"));
            loginButton.Click();
        }

        private void LoginWithEnvironmentalVariables(string userName)
        {
            var userNameTextField = _driver.FindElement(By.Id("username"));
            userNameTextField.TypeText(userName);
            var passwordField = _driver.FindElement(By.Id("password"));
            passwordField.TypeText(Environment.GetEnvironmentVariable($"{userName}_pass"));
            var loginButton = _driver.FindElement(By.XPath("//button[@name='login']"));
            loginButton.Click();
        }

        private void LoginDefaultValues(string userName = "aangelov")
        {
            var userNameTextField = _driver.FindElement(By.Id("username"));
            userNameTextField.TypeText(userName);
            var passwordField = _driver.FindElement(By.Id("password"));
            passwordField.TypeText(Environment.GetEnvironmentVariable($"{userName}_pass"));
            var loginButton = _driver.FindElement(By.XPath("//button[@name='login']"));
            loginButton.Click();
        }

        private void IncreaseProductQuantity()
        {
            var quantityBox = _driver.FindElement(By.CssSelector("[class*='input-text qty text']"));
            quantityBox.TypeText("2");

            Thread.Sleep(2000);
            var updateCart = _driver.FindElement(By.CssSelector("[value*='Update cart']"));
            updateCart.Click();
            Thread.Sleep(4000);

            var totalSpan = _driver.FindElement(By.XPath("//*[@class='order-total']//span"));
            Assert.AreEqual("114.00€", totalSpan.Text);
        }

        private void ApplyCoupon()
        {
            var couponCodeTextField = _driver.FindElement(By.Id("coupon_code"));
            couponCodeTextField.TypeText("happybirthday");

            var applyCouponButton = _driver.FindElement(By.CssSelector("[value*='Apply coupon']"));
            applyCouponButton.Click();

            Thread.Sleep(2000);
            var messageAlert = _driver.FindElement(By.CssSelector("[class*='woocommerce-message']"));
            Assert.AreEqual("Coupon code applied successfully.", messageAlert.Text);
        }

        private void AddRocketToShoppingCart()
        {
            _driver.GoToUrl("http://demos.bellatrix.solutions/");

            var addToCartFalcon9 = _driver.FindElement(By.CssSelector("[data-product_id*='28']"));
            addToCartFalcon9.Click();
            var viewCartButton = _driver.FindElement(By.CssSelector("[class*='added_to_cart wc-forward']"));
            viewCartButton.Click();
        }

        private string GetUserPasswordFromDb(string userName)
        {
            return "@purISQzt%%DYBnLCIhaoG6$";
        }

        private string GenerateUniqueEmail()
        {
            return $"{Guid.NewGuid()}@berlinspaceflowers.com";
        }
    }
}
