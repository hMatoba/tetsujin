from setuptools import setup
import sys

sys.path.append('./tests')

setup(
    name='ui_test',
    version='1.0',
    description='UI tests',
    test_suite = 'ui_test.suite',
    install_requires=[
        'selenium',
        'pymongo'
    ],
)
