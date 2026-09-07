keep it short
don't use filler words
at the end of each conversation, end with the text 'COMPLETED' to indicate that you are waiting for input, do not output COMPLETED if there is  any processing in progress. Only once waiting for user input. 

when a message results in one of more files being modified, increment the version patch number of the item being modified  by one. e.g. 0.1.51 -> 0.1.52, 0.1.52 -> 0.1.53. This excludes objects that have sync'd version numbers where both will be given the updated version number

If a build is required always explicitly identify the version of the build in the message announcing the build

whenever a new functional feature request is defined or modified, append that feature to the file features.md together with a date-time stamp

never introduce changes not directly related to the current request.  If you identify improvements that can be made to the code base, identify these. Do not unilaterally implement them withou explicit instruction to do so.


For existing automation flows: preserve proven behavior, patch minimally, and do not substitute control or detection methods by inference.

Before changing script logic, prefer preserving the existing mechanism over substituting a different one.



Do not replace one detection or control strategy with another unless one of these is true:
1. The user explicitly requested the change.
2. The current strategy is proven broken by evidence from the current repo or logs.
3. The repo already contains a newer authoritative version showing that replacement.

When fixing a regression, make the narrowest change that addresses the observed failure.

Do not broaden the fix into adjacent refactors or consistency changes unless explicitly requested.

If there is a working named setting or asset path already used for a purpose, prefer keeping that mechanism rather than replacing it with a new one.

Example: if LoadMoreJobsImagePath exists and is part of the working flow, do not replace it with text-based targeting without explicit approval.

 Never infer that a newer helper function should replace an older working mechanism unless the repo clearly shows that migration was intended.

You will not make arbitrary changes to the code base to make "improvements" without explicit instructions to do so. You may make suggestions on how to improve the code and/or mitigate potential issues and inefficiencies based on your analysis of the code. You may not unilaterally act to implement those suggestions or improvements. This avoids the code being changed in ways that I am not aware of, which can cause misunderstandings and sometimes fundamental design flaws.
 
 for the winforms app the trigger tab threshold values should use a decimal point. not a comma. Replace comma with decimal point if found.  Ignore the system's regional settings for this. THe same applies to the console app setting. Replaces with a decimal point If a comma is found in this field.
 
 At then end of a message response that resulted in changes build a debug executable. Most testing will be done using desktop shortcuts. If a debug build fails due to errors such as being blocked by a running debug build, report the error and be ready to do the build.  

After every macOS build, explicitly remind the user that Accessibility permissions may need to be updated for the newly built AvBot application.

When reaching the build phase, if a running debug AvBot build blocks the build, kill that running debug build and continue the debug build. If a debug build was killed for this reason, restart the debug build after the build completes unless the user explicitly instructs otherwise.
 
 Always use the default debug build path. 

When building,  wait until you are ready to build to check whether the build is blocked.

winform and runner versions should always match, even when one did not change, at least the version should be updated to keep them in sync.

As much as is possible, the processing of the target page and the extracted data should always be identical for both winform and runner apps.

If this document contains contradictions identify them so that these can be resolved.

If there are circumstances where its requirements should not be adhered to, always identify these.

