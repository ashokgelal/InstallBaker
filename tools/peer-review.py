#!/usr/bin/python

import os.path 
import subprocess
import shlex
import re
import yaml
import _thread

#____________________________________________________________________________
class Comment(object):
    '''container for a an author's note'''
    def __init__(self, author, text):
        self.myAuthor = author
        self.myText = text
    def printSelf(self):
        print('Author: %s' % self.myAuthor)
        print('%s' % self.myText)

#____________________________________________________________________________
class CommentSet(object):
    '''container for a set of notes on a file area'''
    def __init__(self, startLine):
        self.myStartLine = startLine
        self.myCommentList = []
    def append(self, comment):
        self.myCommentList.append(comment)
    def printSelf(self):
        print('________________________________________')
        print('Line:   %s' % self.myStartLine);
        for i in self.myCommentList:
            i.printSelf()

#____________________________________________________________________________
class Commit(object):
    '''container for a review between file deltas'''
    def __init__(self, commitSHA, previousSHA, author, email, date, notes):
        self.myCommitSHA = commitSHA
        self.myPreviousSHA = previousSHA
        self.myAuthor = author
        self.myEmail = email
        self.myDate = date
        self.myNotes = notes
        self.myCommentSetLineNumberList = []
        self.myCommentSetDictionary = {}

    def printSelf(self):
        print('________________________________________')
        print('________________________________________')
        print('SHA1:   %s' % self.myCommitSHA)
        #print('PREV:   %s' % self.myPreviousSHA)
        print('Author: %s' % self.myAuthor)
        print('Email:  %s' % self.myEmail)
        print('Date:   %s' % self.myDate)
        sumComments = 0
        for i in self.myCommentSetLineNumberList:
            sumComments += len(self.myCommentSetDictionary[i].myCommentList)
        print('** %d Comments **' % sumComments)
        print('Notes:  %s' % self.myNotes)

    def printComments(self):
        self.myCommentSetLineNumberList.sort()
        for i in self.myCommentSetLineNumberList:
            self.myCommentSetDictionary[i].printSelf()
    
#____________________________________________________________________________
class ReviewFile(object):
    '''container for all reviews for a file (contains all DeltaReviews)'''
    def __init__(self, filename):
        print('in ReviewFile::init()')
        self.myFilename = filename
        #base, suffix = os.path.splitext(filename)
        self.myReviewFile = filename + '.rvw'
        self.myCommitList = []
        self.myCommitDict = {}

        self.commitRE = re.compile('commit (?P<commitSHA>.*)')
        self.authorRE = re.compile('Author: (?P<author>.*) <(?P<email>.*)>')
        self.dateRE = re.compile('Date:   (?P<date>.*)')
    def printSelf(self):
        for commit in self.myCommitList:
            self.myCommitDict[commit].printSelf()
    def printAll(self):
        for i in self.myCommitList:
            commit = self.myCommitDict[i]
            commit.printSelf()
            commit.printComments()
    def write(self):
        '''write to yaml file'''
        f = open(self.myReviewFile, 'w')
        f.truncate(0)
        f.write(yaml.dump(self))
        f.close()
    def parseNewCommits(self):
        previousSHA = None
        currentSHA = None
        nextSHA = None
        author = None
        email = None
        date = None
        notes = ''

        command = 'git log --reverse --no-merges %s' % self.myFilename
        args = shlex.split(command)
        output = subprocess.check_output(args)
        for line in output.splitlines():
            line = line.decode('utf-8').strip()

            matchFound = False

            match = self.commitRE.match(line)
            if match:
                matchFound = True
                dict = match.groupdict()
                nextSHA = dict['commitSHA']
                # have not collected information for the first commit
                if currentSHA == None:
                    currentSHA = nextSHA
                # consecutive commits
                else:
                    if not currentSHA in self.myCommitList:
                        print('found new commit: %s' % currentSHA)
                        if len(self.myCommitList) > 0:
                            previousSHA = self.myCommitList[len(self.myCommitList) - 1]
                        thisCommit = Commit(currentSHA, previousSHA, author, email, date, notes)
                        self.myCommitList.append(currentSHA)
                        self.myCommitDict[currentSHA] = thisCommit
                            
                    previousSHA = currentSHA
                    currentSHA = nextSHA
                    nextSHA = None
                    notes = ''

            if not matchFound:
                match = self.authorRE.match(line)
                if match:
                    matchFound = True
                    dict = match.groupdict()
                    author = dict['author']
                    email = dict['email']

            if not matchFound:
                match = self.dateRE.match(line)
                if match:
                    matchFound = True
                    dict = match.groupdict()
                    date = dict['date']

            if not matchFound:
                if line != '':
                    notes += line
                    notes += '\n'
        
        if not currentSHA in self.myCommitList:
            print('found new commit: %s' % currentSHA)

            if len(self.myCommitList) > 0:
                previousSHA = self.myCommitList[len(self.myCommitList) - 1]
            thisCommit = Commit(currentSHA, previousSHA, author, email, date, notes)
            self.myCommitList.append(currentSHA)
            self.myCommitDict[currentSHA] = thisCommit
                            


#____________________________________________________________________________
REVIEW_FILE = None
COMMIT = None
LINE_NUMBER = None

def Load(filename):
    '''load a ReviewFile from a yaml file'''
    if os.path.isfile (filename):
        f = open(filename, 'r')
        return yaml.load(f)

def Main(filename):
    global REVIEW_FILE

    #base, suffix = os.path.splitext(filename)
    reviewFilename = filename + '.rvw'
    REVIEW_FILE = Load(reviewFilename)

    # check for correct yaml load
    if not type(REVIEW_FILE) == ReviewFile:
        # create a new one if no yaml load
        REVIEW_FILE = ReviewFile(filename)

    # collect any new commits since the last yaml load
    REVIEW_FILE.parseNewCommits()

    MainLoop()

def MainLoop():
    global REVIEW_FILE
    global COMMIT

    commands = { 
            1: 'list commits',
            2: 'list all comments for all commits',
            3: 'select commit',
            4: 'diff',
            5: 'review menu',
            6: 'save',
            7: 'quit',
            }
    keys = list(commands.keys())
    keys.sort()

    while True:
        print('\n*** Main Menu ***')
        if COMMIT != None:
            print('commit: %s' % COMMIT.myCommitSHA)
            #print('commit: %s' % COMMIT.__dict__)
            #print(COMMIT.myCommentSetLineNumberList)
        for i in keys:
            print('  %d: %s' % (i, commands[i]))
        cmd = input('What now> ')

        try:
            cmd = int(cmd)
        except:
            continue

        if (int(cmd)) in keys:
            RunCommand(commands[int(cmd)])

def ReviewLoop():
    global REVIEW_FILE
    global COMMIT
    global LINE_NUMBER

    commands = { 
            1: 'list comments for commit',
            2: 'list comments for line',
            3: 'select line (0 for file wide comment)',
            4: 'add comment',
            }
    keys = list(commands.keys())
    keys.sort()

    while True:
        print('\n*** Review Menu ***')
        if LINE_NUMBER != None:
            print('line number %s' % LINE_NUMBER)
        for i in keys:
            print('  %d: %s' % (i, commands[i]))
        cmd = input('What now>> ')

        # go back 
        if cmd == '':
            LINE_NUMBER = None
            return

        try:
            cmd = int(cmd)
        except:
            continue

        if (int(cmd)) in keys:
            RunCommand(commands[int(cmd)])

def RunCommand(cmd):
    global REVIEW_FILE
    global COMMIT
    global LINE_NUMBER

    #____________________________________
    if cmd == 'list commits':
        REVIEW_FILE.printSelf()
        return
    #____________________________________
    if cmd.startswith('list all comments for'):
        REVIEW_FILE.printAll()
    #____________________________________
    if cmd == 'save':
        REVIEW_FILE.write()
    #____________________________________
    if cmd == 'quit':
        REVIEW_FILE.write()
        raise SystemExit(0)
    #____________________________________
    if cmd == 'diff':
        if COMMIT == None:
            RunCommand('select commit')
            if COMMIT == None:
                print('you must first select a commit')
                return
        _thread.start_new_thread(CallDiff, ())
        return
    #____________________________________
    if cmd == 'select commit':
        commitNumber = input('5 characters of commit SHA1>\n')
        if len(commitNumber) >= 5:
            for i in REVIEW_FILE.myCommitList:
                if i.startswith(commitNumber):
                    COMMIT = REVIEW_FILE.myCommitDict[i]
                    break;
    #____________________________________
    if cmd == 'review menu':
        if COMMIT == None:
            RunCommand('select commit')
            if COMMIT == None:
                print('you must first select a commit')
                return
        ReviewLoop()
        return
    #____________________________________

    if cmd == 'list comments for commit':
        COMMIT.printComments()
    #____________________________________
    if cmd == 'list comments for line':
        if LINE_NUMBER == None:
            RunCommand('select line ')
        if LINE_NUMBER in COMMIT.myCommentSetLineNumberList:
            COMMIT.myCommentSetDictionary[LINE_NUMBER].printSelf()
    #____________________________________
    if cmd.startswith('select line'):
        num = input('line number> ')
        try:
            num = int(num)
        except:
            num = 0
            print('line number set to 0')
        LINE_NUMBER = num
    #____________________________________
    if cmd == 'add comment':
        if LINE_NUMBER == None:
            RunCommand('select line ')

        author = ''
        while author == '':
            author = input('author>')

        commentText = ''
        additionalLine = None
        print('add comments, blank line to quit')
        count = 0
        while True:
            additionalLine = input('>')
            if additionalLine == '':
                if count == 0:
                    return
                else:
                    break;
            count += 1
            commentText += additionalLine
            commentText += '\n'

        if LINE_NUMBER in COMMIT.myCommentSetLineNumberList: 
            commentSet = COMMIT.myCommentSetDictionary[LINE_NUMBER]
        else:
            COMMIT.myCommentSetLineNumberList.append(LINE_NUMBER)
            commentSet = CommentSet(LINE_NUMBER)
            COMMIT.myCommentSetDictionary[LINE_NUMBER] = commentSet

        comment = Comment(author, commentText)
        commentSet.append(comment)

def CallDiff():
    global REVIEW_FILE
    gitDiffCommand = 'git diff %s %s %s ' % (
            COMMIT.myPreviousSHA, COMMIT.myCommitSHA, REVIEW_FILE.myFilename)
    args = shlex.split(gitDiffCommand)
    subprocess.call(args)


if __name__ == '__main__':
    import optparse

    p = optparse.OptionParser()
    p.add_option('-f', '--filename', action='store', dest='filename')
    p.set_defaults(
        filename='',
    )

    opt, args = p.parse_args()

    if not os.path.isfile (opt.filename):
        print('bad filename')
        raise SystemExit(1)

    Main(opt.filename)
