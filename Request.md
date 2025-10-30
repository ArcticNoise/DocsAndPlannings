
## Project: Docs and Plannings
Need to build the web app which will give us a way to write and store documentation and also plan and track development.

## Technology stack
Using ASP.net C# with SQLite as database. 

## Must have
1. **App should have REST api.**
2. **We should have 2 different tools on web app: documents and planning (analog Jira and Confluence).**
3. **App should have its own registration/authorization system to track users (not a google OAUTH but it can be added later)**
4. **Documents should support markdown and screenshot insert**
5. **Planning should allow to create projects**
6. **Hierarchy of the planning: Project->Epics->Tasks/bugs->subtasks**
7. **Basic statuses for Epics->Tasks/bugs->Subtasks should be: TODO, IN PROGRESS, DONE, CANCELLED, BACKLOG**
8. **User should be able to add more statuses for specific types separately (for tasks only for example)**
9. **All planning items should have assignee section where we can assign it to registered user**
10. **All planning items should have their unique id based on project like in Jira**
11. **Each item should be able to open in separate tab of the browser**
12. **Items should have Summary(title), description and similar fields from jira**
13. **Each project should have kanban board with items show there in corresponding columns**
14. **User should be able to drag and drop tasks on kandan board to corresponding columns**
15. **It should be possible to open item by clicking on its id**