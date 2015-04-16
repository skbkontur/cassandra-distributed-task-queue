import elasticsearch
import curator
import time

days = 8
oldIndexName = "old-data"
dateFormat = "%Y.%m.%d"
indexNamexFormat = "monitoring-index-{DATE}"
oldAliasNameFormat = "{INDEX}-old"
searchAliasNameFormat = "{INDEX}-search"

indexFormat = indexNamexFormat.replace("{DATE}", dateFormat)

client = elasticsearch.Elasticsearch()

indices = curator.get_indices(client)
_filterDate = curator.build_filter(kindOf='older_than', value=days, timestring=dateFormat, time_unit="days")
indices = curator.apply_filter(indices, **_filterDate)
#todo only prefix supported in indexNamexFormat
_filterPrefix = curator.build_filter(kindOf='prefix', value=indexNamexFormat.replace("{DATE}", ""))
indices = curator.apply_filter(indices, **_filterPrefix)

for item in indices:
   
   searchAlias = searchAliasNameFormat.replace("{INDEX}", item)
   oldAlias = oldAliasNameFormat.replace("{INDEX}", item)
   print item
   #print "search: " + searchAlias
   #print "old: " + oldAlias
   curator.alias(client, oldIndexName, alias=oldAlias)
   curator.alias(client, item, alias=oldAlias, remove=True)

   curator.alias(client, oldIndexName, alias=searchAlias)
   curator.alias(client, item, alias=searchAlias, remove=True)
   curator.close(client, item)


















   

