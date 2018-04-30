import unittest
import home_test
import master_test


def suite():
    """run tests"""
    suite = unittest.TestSuite()
    suite.addTests([
        unittest.makeSuite(home_test.HomeTests),
        unittest.makeSuite(master_test.MasterTests),
    ])
    return suite


if __name__ == '__main__':
    unittest.main()
