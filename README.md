# Easy Approach for a Material Requirements Planning (MRP)
## Description
Most Enterprise Resource Planning (ERP) solutions offer some kind of algorithm for calculating material requirements that need to be procured again. In this post I would like to show a hands-on approach written in C# for calculating the material requirements for multi-level items - so items, that will be assembled out of other components. Therefor, we need to break down the BOM and multiply it’s demand to the lower levels. This can be done with the help of a more or less complex matrix (two-dimensional array) and a simple vector (one-dimensional array).

The following steps will be made:
* Build some demo-data
    * eleven items are created with the help of a struct
    * some of them contains a bill of material (BOM)
    * three of them got a demand
* Build the demand-matrix
* Build a unit-matrix
* Invert and normalize the demand-matrix
* Multiply the demand-matrix with the demand-vector to get the total demands

## Substack
Check out the whole blogpost on [Substack](https://kleink.substack.com/p/material-requirements-planning-mrp).