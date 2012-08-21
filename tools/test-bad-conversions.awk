BEGIN {
  convert = "Convert.To(Double|Decimal|Float)"
  culture = "CultureInfo.InvariantCulture"
  good_string = convert && culture
  bad_string = convert && !culture
}

$0 ~ convert {
  if ($0 !~ culture)
  {
    printf ("QUESTIONABLE CONVERT: (%s %d)\n", FILENAME, FNR)
  }
}
