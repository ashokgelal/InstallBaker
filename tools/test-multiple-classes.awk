BEGIN {
  in_c_style_comment = 0
  last_test_fixture_index = 0
  class = "^ *(public |private |protected |internal |protected internal )?(partial )?(static )?class "
}

function test_at_eof() {
  if (depth_1_fn_cnt > 1)
  {
    basename = this_file
    sub(/.*\//, "", basename)
    printf("MULTIPLE CLASSES: %s\n", basename)
  }
}

FNR == 1 {
  test_at_eof()
  this_file = FILENAME
  block_depth = 0
  depth_1_fn_cnt = 0
}

/\/\*/ { in_c_style_comment = 1 }
/\*\// { in_c_style_comment = 0 }

/\{/ { !is_comment() && block_depth++ }
/\}/ { !is_comment() && block_depth-- }

/\[TestFixture\]/ { last_test_fixture_index = NR }

$0 ~ class {
  #print_class()
  #print(block_depth)
  test_depth_of_class()
}

function is_test_fixture()
{
  if (last_test_fixture_index == (NR - 1))
    return 1
  return 0
}

function is_comment()
{
  if (in_c_style_comment)
    return 1
  if (match($0, "^ *//"))
    return 1
  return 0
}

function test_depth_of_class()
{
  if (is_comment())
    return
  if (is_test_fixture())
    return
  if (block_depth == 1)
  {
    depth_1_fn_cnt++
    #print_class()
  }
}

function print_class()
{
  if (is_comment())
    return
  for (i = 1; i <= NF; i++) 
    if ($i == "class") 
    {
      print($(i+1))
      break
    }
}
