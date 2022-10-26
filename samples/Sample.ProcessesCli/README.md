## BabyKusto.ProcessQuerier

A trivial example of using BabyKusto with real data,
serving as a tool to explore live processes running on the current machine
with KQL queries.

What follows is an example of the output produced by running this sample.
It serves as a REPL and new queries can be evaluated at will.


```
/----------------------------------------------------------------\
| Welcome to BabyKusto.ProcessQuerier. You can write KQL queries |
| and explore the live list of processes on your machine.        |
\----------------------------------------------------------------/

Example: counting the total number of processes:
> Processes | count

Count:long
------------------
257

Example: Find the process using the most memory:
> Processes | project name, memMB=workingSet/1024/1024 | order by memMB desc | take 1

name:string; memMB:long
------------------
MsMpEng; 1500

>
```
