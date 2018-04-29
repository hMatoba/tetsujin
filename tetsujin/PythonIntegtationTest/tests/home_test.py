import unittest
import os
import lxml.html
import requests
import pymongo

# requestsが不全な証明書に出すメッセージの抑制
import urllib3
from urllib3.exceptions import InsecureRequestWarning
urllib3.disable_warnings(InsecureRequestWarning)
 
# このテストは開発環境、CI環境ともにコンテナ内ではなくホストから行う
# そのためにアプリを動かすコンテナのポートがホストにバインドされている
HOST = "https://127.0.0.1"

OK_CODE = 200

class HomeTests(unittest.TestCase):
    def test_all_links_on_top_are_fine(self):
        """top page links"""
        response = requests.get(HOST + "/", verify=False)
        self.assertEqual(OK_CODE, response.status_code)
        dom = lxml.html.fromstring(response.text)
        link_tags = dom.xpath("//a")
        for link_tag in link_tags:
            href = link_tag.get('href')
            if href is not None and not href.startswith("http"):
                _response = requests.get(HOST + href, verify=False)
                self.assertEqual(OK_CODE, _response.status_code)