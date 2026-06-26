private void RefreshTasks()
{
    if (gridTasks == null)
    {
        return;
    }

    gridTasks.DataSource = database.GetTasksTable();

    if (gridTasks.Columns.Contains("TaskID"))
    {
        gridTasks.Columns["TaskID"].Width = 70;
    }
}