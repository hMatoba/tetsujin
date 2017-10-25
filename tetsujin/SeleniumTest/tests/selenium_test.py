import unittest
import os

from selenium import webdriver
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.common.desired_capabilities import DesiredCapabilities
import pymongo

HOST = "https://proxy"

def prepare_db():
    if 'TRAVIS' in os.environ:
        host = '127.0.0.1'
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
        self.driver.get(HOST + "/")
        self.assertIn("Absurd", self.driver.title)

    def test_login_success(self):
        """login success"""
        is_authenticated = (
            LoginFormStory(self.driver)
                .enter_id("testuser")
                .enter_password("password")
                .post_form()
                .is_authenticated()
        )
        self.assertTrue(is_authenticated)

    def test_login_failure(self):
        """login failure"""
        is_authenticated = (
            LoginFormStory(self.driver)
                .enter_id("fooooooo")
                .enter_password("barrrrrrrrrr")
                .post_form()
                .is_authenticated()
        )
        self.assertFalse(is_authenticated)

class LoginFormStory:
    def __init__(self, driver):
        driver.get(HOST + "/Auth/Login")
        self._driver = driver

    def enter_id(self, id_):
        el = self._driver.find_element_by_name("_id")
        el.send_keys(id_)
        return self

    def enter_password(self, password):
        el = self._driver.find_element_by_name("password")
        el.send_keys(password)
        return self

    def post_form(self):
        el = self._driver.find_element_by_name("enter")
        el.click()
        return self

    def is_authenticated(self):
        if "Admin Page" in self._driver.title:
            return True
        else:
            return False

def suite():
    """run tests"""
    suite = unittest.TestSuite()
    suite.addTests([
        unittest.makeSuite(BrowserTests),
    ])
    return suite


if __name__ == '__main__':
    unittest.main()
