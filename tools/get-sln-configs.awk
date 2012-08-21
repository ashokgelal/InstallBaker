BEGIN {
     beginTarget = "GlobalSection.SolutionConfigurationPlatforms."
     endTarget="EndGlobalSection"
     in_target = 0
 }

 $1 ~ endTarget {
     if (in_target)
         in_target = 0
 }

 /.*/ {
     if (in_target)
         print($1)
 }

 $1 ~ beginTarget {
     in_target = 1
 }

