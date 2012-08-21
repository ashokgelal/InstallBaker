__author__ = 'Josh'

import cpplint
from os import walk, path

extensions = ['.cpp', '.h']
lintable_files = []

for root, dirs, files in walk('../'):
    for file in files:
        for ext in extensions:
            if file.endswith(ext):
                if root.find('boost') != -1:
                    break;
	    	if root.find('AirPcapHandler\Include') != -1:
		    break;
                lintable_files.append(path.join(root, file))
                print 'Appending %s' %root
                break

cpplint.mglint(lintable_files)
