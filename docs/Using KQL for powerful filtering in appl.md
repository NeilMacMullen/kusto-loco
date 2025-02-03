Using KQL for powerful filtering in applications

One of the challenges for almost any LoB application is providing filtering and querying 
capabiliity for tabular data.

Not ust LoB - think of a games engine where you want to filter items in an inventory slot or 
a packet-analyser 

Typically you will see this kind of display (devops screen shot here)
.. a long list of dropdowns and checkboxes.

These are great when you have fairly well defined search criteria but there's always a power-user who wants to do more.

You could start to generate expression trees or write a customer parser.

Or... you could use a standard query language 

SQL --sure but not very user=friendly, need to be careful about injection.

KQL....

supports combinatorial operators,
reasonably user-friendly ?
powerful, 
query-only

Downsides... supports projection and data-reshaping...
normally only useable in ADX
Easilyl composable - i.e drive from a UI

Challenges... mutability and nesting structures
Identity column


Provide a worked example/sample


TODO... 
- operator filtering
- DTO re-casting, 
- Id projection (requires check to be applied to user section)