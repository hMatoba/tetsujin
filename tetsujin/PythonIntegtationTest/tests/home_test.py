import unittest
import os
import requests

# requestsが不全な証明書に出すメッセージの抑制
#import urllib3
#from urllib3.exceptions import InsecureRequestWarning
#urllib3.disable_warnings(InsecureRequestWarning)
 
# このテストは開発環境、CI環境ともにコンテナ内ではなくホストから行う
# そのためにアプリを動かすコンテナのポートがホストにバインドされている
HOST = "https://127.0.0.1"

OK_CODE = 200

class HomeTests(unittest.TestCase):
    def test_top_is_fine(self):
        """top page links"""
        response = requests.get(HOST + "/", verify=False)
        self.assertEqual(OK_CODE, response.status_code)
