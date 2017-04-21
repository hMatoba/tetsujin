import unittest

from selenium import webdriver
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.common.by import By
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import TimeoutException
from selenium.webdriver.common.desired_capabilities import DesiredCapabilities

HOST = "http://192.168.11.17:32768"
DELAY = 5

class BrowserTests(unittest.TestCase):
    """tests for main five functions."""

    def setUp(self):
        self.driver = webdriver.Remote(
            command_executor='http://127.0.0.1:4444/wd/hub',
            desired_capabilities=DesiredCapabilities.CHROME)
        self.driver.implicitly_wait(10)

    def test_top(self):
        """top page"""
        self.driver.get(HOST + "/")

        self.assertIn("tetsujin", self.driver.title)


    def test_login_success(self):
        """login success"""
        self.driver.get(HOST + "/Login")

        el1 = self.driver.find_element_by_name("_id")
        el1.send_keys("testuser")

        el2 = self.driver.find_element_by_name("password")
        el2.send_keys("password")

        el3 = self.driver.find_element_by_name("enter")
        el3.click()

        self.assertEqual("Admin Menu", self.driver.title)

    def test_login_failure(self):
        """login failure"""
        self.driver.get(HOST + "/Login")

        el1 = self.driver.find_element_by_name("_id")
        el1.send_keys("testuser")
        el2 = self.driver.find_element_by_name("password")
        el2.send_keys("fooooooo")
        el3 = self.driver.find_element_by_name("enter")
        el3.click()

        self.assertNotEqual("Admin Menu", self.driver.title)


def suite():
    """run tests"""
    suite = unittest.TestSuite()
    suite.addTests([unittest.makeSuite(BrowserTests),
                    ])
    return suite


if __name__ == '__main__':
    unittest.main()