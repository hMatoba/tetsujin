from setuptools import setup
import sys

sys.path.append('./tests')

setup(
    name='browsertest',
    version='1.0',
    description='test a project on browser',
    test_suite = 's_test.suite',
    install_requires=[
        'selenium',
        'pymongo'
    ],
)
