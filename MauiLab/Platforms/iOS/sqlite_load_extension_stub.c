/* Apple's libsqlite3 does not export sqlite3_enable_load_extension.
   The managed provider still references it, so the link needs a definition.
   Loading extensions is not used; report an error if something calls it. */
int sqlite3_enable_load_extension(void *db, int onoff)
{
	(void)db;
	(void)onoff;
	return 1;
}
