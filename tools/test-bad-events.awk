BEGIN {
  access = "^ *(public |private |protected |internal |protected internal )?"

  event_keyword = "event "
  private_field = "_.*"
  event_handler = "EventHandler" 
  generic = "(<.*>)? "
  void_keyword = "void "

  event_field = access event_handler generic private_field
  event = access event_keyword event_handler

}

/\#region Event Related/ {
  event_field_count = 0
  event_count = 0
}

/\#endregion Event Related/ {
  if (event_field_count != event_count)
  {
    printf("BAD EVENT PATTERN: %s\n", FILENAME)
  }
}

$0 ~ event_field { event_field_count++ }
$0 ~ event { event_count++ }

