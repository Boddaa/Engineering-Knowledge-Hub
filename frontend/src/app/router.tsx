import { createBrowserRouter } from 'react-router-dom';
import { AppLayout } from '@/components/layout/AppLayout';
import {
  HomePage,
  KnowledgePage,
  KnowledgeArticlePage,
  KnowledgeArticleEditorPage,
  IncidentsPage,
  IncidentDetailPage,
  IncidentCreatePage,
  ProjectsPage,
  NotesPage,
  BoardsPage,
  NotFoundPage,
} from '@/pages';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      {
        index: true,
        element: <HomePage />,
      },
      {
        path: 'knowledge',
        element: <KnowledgePage />,
      },
      {
        path: 'knowledge/new',
        element: <KnowledgeArticleEditorPage />,
      },
      {
        path: 'knowledge/:id',
        element: <KnowledgeArticlePage />,
      },
      {
        path: 'knowledge/:id/edit',
        element: <KnowledgeArticleEditorPage />,
      },
      {
        path: 'incidents',
        element: <IncidentsPage />,
      },
      {
        path: 'incidents/new',
        element: <IncidentCreatePage />,
      },
      {
        path: 'incidents/:id',
        element: <IncidentDetailPage />,
      },
      {
        path: 'projects',
        element: <ProjectsPage />,
      },
      {
        path: 'notes',
        element: <NotesPage />,
      },
      {
        path: 'boards',
        element: <BoardsPage />,
      },
      {
        path: '*',
        element: <NotFoundPage />,
      },
    ],
  },
]);
