-- REPLACE {{dbName}} PagesJaunes_Dev
USE [{{dbName}}]
GO





--/* ------------------------------------------------------------------------------------- */
--/* ------------------------------------------------------------------------------------- */
--/* Modules */
--/* ------------------------------------------------------------------------------------- */
--/* ------------------------------------------------------------------------------------- */

--DELETE from [Modules]
--GO
--SET IDENTITY_INSERT [Modules] ON
--GO
--INSERT Into [Modules] ([Id],[NomModule],[AccessRightStoredProcedure]) VALUES (1, 'Personne', 'GetPersonneAccessRights');

--GO
--SET IDENTITY_INSERT [Modules] OFF
--GO



--/* ------------------------------------------------------------------------------------- */
--/* ------------------------------------------------------------------------------------- */
--/* Ressources */
--/* ------------------------------------------------------------------------------------- */
--/* ------------------------------------------------------------------------------------- */

--DELETE from [Ressources]
--GO
--SET IDENTITY_INSERT [Ressources] ON
--GO
--INSERT Into [Ressources] ([Id],[ModuleId],[NomRessource],[NomClasseAssociee]) VALUES (1, 1, 'Personne-PersonneEtatCivil', 'PumaWeb.ReadModel.PersonneEtatCivil');
--INSERT Into [Ressources] ([Id],[ModuleId],[NomRessource],[NomClasseAssociee]) VALUES (2, 1, 'Personne-Photo', 'PumaWeb.ReadModel.PersonnePhoto');
--INSERT Into [Ressources] ([Id],[ModuleId],[NomRessource],[NomClasseAssociee]) VALUES (3, 1, 'Personne-Coordonnees', 'PumaWeb.ReadModel.PersonneCoordonnee');
--INSERT Into [Ressources] ([Id],[ModuleId],[NomRessource],[NomClasseAssociee]) VALUES (4, 1, 'Personne-Images', 'PumaWeb.Domain.Domain.Images.PersonneImage');

--GO
--SET IDENTITY_INSERT [Ressources] OFF
--GO