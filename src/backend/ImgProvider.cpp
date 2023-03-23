#include "ImgProvider.h"

ImgProvider::ImgProvider(FileSystemWalker* const ptrFileSystemWalker)
    : QQuickImageProvider(QQuickImageProvider::Pixmap), m_ptrFileSystemWalker(ptrFileSystemWalker)
{}

QPixmap ImgProvider::requestPixmap(const QString& id, QSize* size, const QSize& requestedSize)
{
    QPixmap pixmap;
    if (id.contains('_'))
    {
        auto ixUnderscore = id.lastIndexOf('_');
        auto command = id.last(id.size() - ixUnderscore - 1);
        if (command == "next")
        {
            pixmap.load(m_ptrFileSystemWalker->getNextFile());
        }
        else if (command == "prev")
        {
            pixmap.load(m_ptrFileSystemWalker->getPrevFile());
        }
        else
        {
            qWarning("Unknown command %s", command.toLatin1().data());
            pixmap.load(m_ptrFileSystemWalker->getCurrentFile());
        }
    }
    else
    {
        pixmap.load(m_ptrFileSystemWalker->getCurrentFile());
    }

    *size = pixmap.size();

    return pixmap;
}
