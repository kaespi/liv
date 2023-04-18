#include "FileSystemWalker.h"

#include "IDir.h"

#include <QFileInfo>

namespace
{
QString composeFullPath(const IDir* ptrDir, const QString& filename)
{
    return QDir::cleanPath(ptrDir->absolutePath() + QDir::separator() + filename);
}

void collectFilesInDir(const IDir* ptrDir, QStringList& pattern, QStringList& files)
{
    files = ptrDir->entryList(pattern, QDir::Files, QDir::Name | QDir::IgnoreCase);
}

void updatePattern(const QList<QByteArray>& extensions, QStringList& pattern)
{
    pattern.clear();
    for (const auto& extension : extensions)
    {
        pattern.append(QString("*.") + extension.toLower());
        pattern.append(QString("*.") + extension.toUpper());
    }
}
} // namespace

FileSystemWalker::FileSystemWalker(const QString& file, const IFileSystemClassesFactory* ptrFsFactory,
                                   QList<QByteArray>& fileTypes)
{
    m_ptrDir = ptrFsFactory->createDirClass();
    m_ptrFileSystem = ptrFsFactory->createFileSystemClass();

    updatePattern(fileTypes, m_filePattern);

    QString filename{};

    if (file.isEmpty())
    {
        qWarning("No filename provided");
        m_ptrDir->setCurrentPath();
    }
    else
    {
        if (m_ptrFileSystem->fileExists(file))
        {
            QFileInfo fileInfo(file);
            if (fileInfo.isFile())
            {
                filename = fileInfo.fileName();
                auto dir = fileInfo.dir();
                m_ptrDir->setPath(dir.canonicalPath());
            }
            else
            {
                m_ptrDir->setPath(file);
            }
        }
        else
        {
            m_ptrDir->setCurrentPath();
            qWarning("File %s doesn't exist", file.toLatin1().data());
        }
    }

    collectFilesInDir(m_ptrDir.get(), m_filePattern, m_filesInDir);

    if (not filename.isEmpty())
    {
        m_currentFileIndex = m_filesInDir.indexOf(filename);
    }
}

QString FileSystemWalker::getCurrentFile() const
{
    if (m_currentFileIndex < m_filesInDir.size())
    {
        return composeFullPath(m_ptrDir.get(), m_filesInDir[m_currentFileIndex]);
    }
    else
    {
        return "";
    }
}

QString FileSystemWalker::getNextFile()
{
    updateFileIndexToExisting(+1);
    return getCurrentFile();
}

QString FileSystemWalker::getPrevFile()
{
    updateFileIndexToExisting(-1);
    return getCurrentFile();
}

void FileSystemWalker::updateFileIndexToExisting(int increment)
{
    if (m_filesInDir.size() <= 1)
    {
        m_currentFileIndex = 0;
        return;
    }

    auto fileIndex = m_currentFileIndex;
    bool foundNonExistingFile = false;
    do
    {
        fileIndex += increment;

        if (fileIndex >= m_filesInDir.size())
        {
            fileIndex = 0;
        }
        else if (fileIndex < 0)
        {
            fileIndex = m_filesInDir.size() - 1;
        }

        if (m_ptrFileSystem->fileExists(composeFullPath(m_ptrDir.get(), m_filesInDir[fileIndex])))
        {
            m_currentFileIndex = fileIndex;
            break;
        }

        foundNonExistingFile = true;
    } while (fileIndex != m_currentFileIndex);

    if (foundNonExistingFile)
    {
        rescanFiles();
    }
}

void FileSystemWalker::rescanFiles()
{
    auto currentFile = m_filesInDir[m_currentFileIndex];
    collectFilesInDir(m_ptrDir.get(), m_filePattern, m_filesInDir);
    m_currentFileIndex = m_filesInDir.indexOf(currentFile);
    if (m_currentFileIndex < 0)
    {
        m_currentFileIndex = 0;
    }
}
