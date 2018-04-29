
from setuptools import setup
import sys

sys.path.append('./tests')

setup(
    name='py_integration_test',
    version='1.0',
    description='integration tests',
    test_suite = 'integration_test_runner.suite',
    install_requires=[
        'lxml',
        'requests',
        'pymongo'
    ],
)
