"""
This creates a command line link to CloudCompare.
www.CloudComapare.org
http://www.danielgm.net/cc/doc/wiki/index.php5?title=CommandLine
"""

import sys
import System.IO
sys.path.append(str(System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory())) + '\Plug-ins\IronPython\Lib')

import subprocess
import os
import shutil
import glob

import clr
clr.AddReferenceToFileAndPath(os.path.join(System.IO.Directory.GetCurrentDirectory(), 'RhinoCommon.dll'))
import Rhino as rc


class CloudCompare:
    def __init__(self):
        self.dir = os.path.normpath('C:\\Program Files\\CloudCompare')
    
    def open(self, filePath):
        """
        Open in CloudCompare.
        """
        if os.path.isdir(self.dir) == True:
            #run CloudCompare cmd line
            Args = self.dir + '\CloudCompare ' + filePath
            P = subprocess.Popen(Args , cwd=self.dir)
            #P.wait()
        else:
            return 'CloudCompare missing. Install CloudCompare in ' + self.dir

    def CCcommand(self, commands):
        """
        write commands for CloudCompare.
        """
        if os.path.isdir(self.dir) == True:
            #create command string
            command = str()
            for com in commands:
                command = command + com + ' '
            #run CloudCompare cmd line
            Args = self.dir + '\CloudCompare ' + command
            P = subprocess.Popen(Args , cwd=self.dir)
            P.wait()
            
            return 'Done. Find the output next to the input file.'
        else:
            return 'CloudCompare missing. Install CloudCompare in ' + self.dir

    def CCcommandE57(self, filePath, commands, outName):
        """
        write commands for CloudCompare.
        """
        if os.path.isdir(self.dir) == True:
            #create command string
            command = str()
            for com in commands:
                command = command + com + ' '
            print command
            #run CloudCompare cmd line
            Args = self.dir + '\CloudCompare -SILENT -O ' + filePath + ' -NO_TIMESTAMP -C_EXPORT_FMT E57 -AUTO_SAVE OFF ' + command + ' -SAVE_CLOUDS ALL_AT_ONCE' 
            P = subprocess.Popen(Args , cwd=self.dir)
            P.wait()
        
            #filePath for output file of CloudCommpare
            tmpFilePath = os.path.join(os.path.dirname(filePath), 'AllClouds.e57')
            
            #outputFilePath
            outputFilePath = os.path.join(os.path.dirname(filePath), outName + '.e57')
            
            #Rename (and move) tmpFilePath
            shutil.move(tmpFilePath, outputFilePath)
            
            return outputFilePath
        else:
            return 'CloudCompare missing. Install CloudCompare in ' + self.dir

    def CCcommandXYZ(self, filePath, commands, outName):
        """
        write commands for CloudCompare.
        """
        if os.path.isdir(self.dir) == True:
            #create command string
            command = str()
            for com in commands:
                command = command + com + ' '
            print command
            #run CloudCompare cmd line
            Args = self.dir + '\CloudCompare -SILENT -O ' + filePath + ' -NO_TIMESTAMP -C_EXPORT_FMT ASC -EXT xyz -AUTO_SAVE OFF -MERGE_CLOUDS ' + command + ' -SAVE_CLOUDS' 
            P = subprocess.Popen(Args , cwd=self.dir)
            P.wait()
        
            #filePath for output file of CloudCommpare
            tmpFilePath = max(glob.iglob(os.path.splitext(filePath)[0]+'*.xyz'), key=os.path.getctime)
            
            #outputFilePath
            outputFilePath = os.path.join(os.path.dirname(filePath), outName + '.xyz')
            
            #Rename (and move) tmpFilePath
            shutil.move(tmpFilePath, outputFilePath)
            
            return outputFilePath
        else:
            return 'CloudCompare missing. Install CloudCompare in ' + self.dir


    def toXYZ(self, filePath):
        """
        Merge and exports an XYZ file through CLoudCompare.
        """
        if os.path.isdir(self.dir) == True:
            #run CloudCompare cmd line
            Args = self.dir + '\CloudCompare -SILENT -O ' + filePath + ' -NO_TIMESTAMP -C_EXPORT_FMT ASC -EXT xyz -MERGE_CLOUDS' 
            P = subprocess.Popen(Args , cwd=self.dir)
            P.wait()
        
            #filePath for output file of CloudCommpare
            tmpFilePath = os.path.splitext(filePath)[0] + '_MERGED.xyz'
            
            #outputFilePath
            outputFilePath = os.path.splitext(filePath)[0] + '.xyz' #os.path.join(os.path.dirname(filePath), outName + '.xyz')
            
            #Rename (and move) tmpFilePath
            shutil.move(tmpFilePath, outputFilePath)
            
            return outputFilePath
        else:
            return 'CloudCompare missing. Install CloudCompare in ' + self.dir


    def SubsampleSpatial(self, filePath, distance, outName):
        """
        Spatially subsamples point cloud through CloudCompare and outputs a filepath.
        Args:   filePath: file to subsample. dist: Distance between points to aim for through the subsampling. outName: name for new file
        Returns:    filePath: file path to new in same directory as input file
        """
        if os.path.isdir(self.dir) == True:
            #run CloudCompare cmd line
            Args = self.dir + '\CloudCompare -SILENT -O ' + filePath + ' -NO_TIMESTAMP -C_EXPORT_FMT E57 -AUTO_SAVE OFF -SS SPATIAL ' + str(distance) + ' -SAVE_CLOUDS ALL_AT_ONCE' 
            P = subprocess.Popen(Args , cwd=self.dir)
            P.wait()
        
            #filePath for output file of CloudCommpare
            tmpFilePath = os.path.join(os.path.dirname(filePath), 'AllClouds.e57')
            
            #outputFilePath
            outputFilePath = os.path.join(os.path.dirname(filePath), outName + '.e57')
            
            #Rename (and move) tmpFilePath
            shutil.move(tmpFilePath, outputFilePath)
            
            return outputFilePath
        else:
            return 'CloudCompare missing. Install CloudCompare in ' + self.dir

    def SubsampleRandom(self, filePath, amount, outName):
        """
        Randomly subsamples point cloud through CloudCompare and outputs a filepath
        Args:   filePath: file to subsample. amount: Amount of points per scan. outName: name for new file
        Returns:    filePath: file path to new in same directory as input file
        """
        if os.path.isdir(self.dir) == True:
            #run CloudCompare cmd line
            Args = self.dir + '\CloudCompare -SILENT -O ' + filePath + ' -NO_TIMESTAMP -C_EXPORT_FMT E57 -AUTO_SAVE OFF -SS RANDOM ' + str(amount) + ' -SAVE_CLOUDS ALL_AT_ONCE' 
            P = subprocess.Popen(Args , cwd=self.dir)
            P.wait()
           
            #filePath for output file of CloudCommpare
            tmpFilePath = os.path.join(os.path.dirname(filePath), 'AllClouds.e57')
           
            #outputFilePath
            outputFilePath = os.path.join(os.path.dirname(filePath), outName + '.e57')
            
            #Rename (and move) tmpFilePath
            shutil.move(tmpFilePath, outputFilePath)
            
        #Output
            return outputFilePath
        else:
            return 'CloudCompare missing. Install CloudCompare in ' + self.dir
