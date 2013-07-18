/** This script corrects the Biota Rank Rules table which erroneously asserts that the only valid child rank of a Plantae SubKingdom is a Phylum, even
though no Phylum rank is defined for Plantae. 

This problem manifests itself as an inability to add ranks below SubKingdom when the Kingdom is Plantae

The script simply changes the valid child of Plantae Subkingdom to be 'Division' 
**/
UPDATE tblBiotaDefRules SET vchrValidChildList='''D'''
WHERE chrKingdomCode = 'P' AND chrRankCode = 'SKING';


