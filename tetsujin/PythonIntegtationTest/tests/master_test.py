import unittest
import os
import lxml.html
import requests
import pymongo
import datetime

# requestsが不全な証明書に出すメッセージの抑制
import urllib3
from urllib3.exceptions import InsecureRequestWarning
urllib3.disable_warnings(InsecureRequestWarning)
 
# このテストは開発環境、CI環境ともにコンテナ内ではなくホストから行う
# そのためにアプリを動かすコンテナのポートがホストにバインドされている
HOST = "https://127.0.0.1"

OK_CODE = 200

def prepare_db():
    if 'TRAVIS' in os.environ:
        host = '127.0.0.1'
    else:
        host = '10.0.75.1'
    client = pymongo.MongoClient(host, 27017)
    db = client.get_database('blog')

    _id = "testuser"
    pw_hash = "0d6a78622e8c92da07a2aa0e34ad7656a644481b849b0b75dc31358a59708251"
    db.Master.update(
        {'_id':_id},
        {'$set':{'pw':pw_hash}},
        True
    )

class MasterTests(unittest.TestCase):
    def setUp(self):
        _id = "testuser"
        pw = "password"
        session = requests.Session()
        session.post(
            HOST + "/Auth/Login",
            {"_id":_id, "password":pw},
            verify = False
        )
        self.session = session
        prepare_db()

    def test_get_master_top(self):
        """master's top page"""
        response = self.session.get(HOST + "/Master")
        self.assertEqual(response.status_code, OK_CODE)
        self.assertEqual(response.url, HOST + "/Master")
