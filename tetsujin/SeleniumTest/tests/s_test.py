import unittest
import os

from selenium import webdriver
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.common.desired_capabilities import DesiredCapabilities
import pymongo

HOST = "https://proxy"

def prepare_db():
    if 'TRAVIS' in os.environ:
        host = 'mymongo'
    else:
        host = '10.0.75.1'
    client = pymongo.MongoClient(host, 27017)
    db = client.get_database('blog')
    db.Master.update( {'_id':'testuser'} ,{'$set':{'pw':'0d6a78622e8c92da07a2aa0e34ad7656a644481b849b0b75dc31358a59708251'}}, True)

class BrowserTests(unittest.TestCase):
    def setUp(self):
        prepare_db()
        self.driver = webdriver.Remote(
            command_executor = 'http://127.0.0.1:4444/wd/hub',
            desired_capabilities = DesiredCapabilities.CHROME
        )
        self.driver.implicitly_wait(10)

    def test_top(self):
        """top page"""
        pass
        self.driver.get(HOST + "/")
        self.assertIn("Absurd", self.driver.title)

    def test_login_success(self):
        """login success"""
        self.driver.get(HOST + "/Auth/Login")

        el1 = self.driver.find_element_by_name("_id")
        el1.send_keys("testuser")

        el2 = self.driver.find_element_by_name("password")
        el2.send_keys("password")

        el3 = self.driver.find_element_by_name("enter")
        el3.click()

        self.assertIn("Admin Page", self.driver.title)

    def test_login_failure(self):
        """login failure"""
        self.driver.get(HOST + "/Auth/Login")

        el1 = self.driver.find_element_by_name("_id")
        el1.send_keys("testuser")
        el2 = self.driver.find_element_by_name("password")
        el2.send_keys("fooooooo")
        el3 = self.driver.find_element_by_name("enter")
        el3.click()

        self.assertNotIn("Admin Page", self.driver.title)

def suite():
    """run tests"""
    suite = unittest.TestSuite()
    suite.addTests([
        unittest.makeSuite(BrowserTests),
    ])
    return suite


if __name__ == '__main__':
    unittest.main()